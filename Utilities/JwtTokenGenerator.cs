using AccessControl_API.Data;
using AccessControl_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccessControl_API.Utilities
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public JwtTokenGenerator(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        public string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!);

            var permissions = _db.UserGroups
                .Where(ug => ug.UserId == user.Id)
                .SelectMany(ug => ug.Group.GroupPermissions)
                .Select(gp => gp.Permission.Name)
                .Distinct()
                .ToList();

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_config["JwtSettings:DurationInMinutes"]!)
                ),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
