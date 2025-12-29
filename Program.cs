using AccessControl_API.Authorization;
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AccessControl_API.Services;
using AccessControl_API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Key"]!);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7154", 
            "http://localhost:5154",
            "https://localhost:7000",
            "http://localhost:5000"
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Entity Framework and SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtTokenGenerator>();

// Register Authorization Handler
builder.Services.AddScoped<PermissionHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionHandler>();

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Define permission-based policies
    var permissions = new[]
    {
        "MANAGE_USERS",
        "CHECK_IN_VISITOR",
        "CHECK_OUT_VISITOR",
        "VIEW_ACTIVE_VISITORS"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});


// Configure AutoMapper For Object Mapping
builder.Services.AddAutoMapper(o =>
{
    // Visit / Access Log mappings
    o.CreateMap<VisitLog, CheckInDTO>().ReverseMap();
    o.CreateMap<VisitLog, VisitLogResponseDTO>()
        .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User.FirstName))
        .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.User.LastName))
        .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
        .ForMember(dest => dest.UserIdentificationNumber, opt => opt.MapFrom(src => src.User.IdentificationNumber));

    // User mappings
    o.CreateMap<User, UserDTO>()
        .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.UserGroups.FirstOrDefault() != null ? src.UserGroups.FirstOrDefault()!.Group.Name : ""))
        .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.UserGroups.FirstOrDefault() != null ? src.UserGroups.FirstOrDefault()!.GroupId : 0));
    o.CreateMap<User, UserCreateUpdateDTO>().ReverseMap();
    o.CreateMap<UserCreateUpdateDTO, User>();

    // Group mappings
    o.CreateMap<Group, GroupDTO>().ReverseMap();

    // Permission mappings
    o.CreateMap<Permission, PermissionDTO>().ReverseMap();

});


var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply migrations
    context.Database.Migrate();

    // Seed data (only runs if database is empty)
    DbSeeder.Seed(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Configure Scalar API Reference
    app.MapScalarApiReference(options =>
    {
        options.Title = "AccessControl API Documentation";
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Enable CORS
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();