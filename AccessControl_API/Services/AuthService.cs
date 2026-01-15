using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext db, IMapper mapper, JwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _mapper = mapper;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            try
            {
                // Check if any user with the given email exists
                return await _db.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception)
            {
                // Log exception details here if necessary
                throw;
            }
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                // Find user by email with groups and permissions
                var user = await _db.Users
                    .Include(u => u.UserGroups)
                        .ThenInclude(ug => ug.Group)
                            .ThenInclude(g => g.GroupPermissions)
                                .ThenInclude(gp => gp.Permission)
                    .FirstOrDefaultAsync(u => u.Email == loginRequestDTO.Email);

                if (user == null)
                {
                    return new LoginResponseDTO 
                    { 
                        Result = LoginResult.InvalidCredentials,
                        Message = "Invalid email or password."
                    };
                }

                // Verify password
                if (!PasswordHasher.Verify(loginRequestDTO.Password, user.PasswordHash))
                {
                    return new LoginResponseDTO 
                    { 
                        Result = LoginResult.InvalidCredentials,
                        Message = "Invalid email or password."
                    };
                }

                // Check if user account is approved
                if (!user.IsApproved)
                {
                    return new LoginResponseDTO 
                    { 
                        Result = LoginResult.AccountNotApproved,
                        Message = "Your account is pending approval by an administrator. You will be notified once your account is approved."
                    };
                }

                // Get user permissions
                var permissions = user.UserGroups
                    .SelectMany(ug => ug.Group.GroupPermissions)
                    .Select(gp => gp.Permission.Name)
                    .Distinct()
                    .ToList();

                // Generate JWT token
                var token = _jwtTokenGenerator.GenerateToken(user);

                // Return login response with user data, token, and permissions
                return new LoginResponseDTO
                {
                    User = _mapper.Map<UserDTO>(user),
                    Token = token,
                    Permissions = permissions,
                    Result = LoginResult.Success,
                    Message = "Login successful."
                };
            }
            catch (Exception)
            {
                // Log exception details here if necessary
                throw;
            }
        }

        public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                // Check if email already exists
                var emailExists = await IsEmailExistAsync(registrationRequestDTO.Email);
                if (emailExists)
                {
                    // Log for debugging
                    Console.WriteLine($"Email already exists: {registrationRequestDTO.Email}");
                    return null; // Email already exists
                }

                // Verify that the group exists
                var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == registrationRequestDTO.GroupId);
                if (group == null)
                {
                    // Log for debugging
                    Console.WriteLine($"Invalid group ID: {registrationRequestDTO.GroupId}");
                    return null; // Invalid group ID
                }

                // Determine if approval is needed based on group
                bool requiresApproval = group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                       group.Name.Equals("Security", StringComparison.OrdinalIgnoreCase);

                // Create new user
                var user = new User
                {
                    FirstName = registrationRequestDTO.FirstName,
                    LastName = registrationRequestDTO.LastName,
                    Email = registrationRequestDTO.Email,
                    IdentificationNumber = registrationRequestDTO.IdentificationNumber,
                    PasswordHash = PasswordHasher.Hash(registrationRequestDTO.Password),
                    IsApproved = !requiresApproval // Auto-approve if not Admin/Security
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Assign user to the specified group
                var userGroup = new UserGroup
                {
                    UserId = user.Id,
                    GroupId = registrationRequestDTO.GroupId
                };

                _db.UserGroups.Add(userGroup);
                await _db.SaveChangesAsync();

                // Map User to UserDTO and return
                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                // Log exception details here
                Console.WriteLine($"Registration error: {ex.Message}");
                throw;
            }
        }
    }
}
