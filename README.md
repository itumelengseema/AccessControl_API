# AccessControl System

A **Role-Based Access Control (RBAC) System** with RESTful API and Web Application built with **ASP.NET Core (.NET 10)** for managing users, groups, permissions, and visitor access control.

## 🌟 Features

### Backend API
- **User Management** - Complete CRUD operations with authentication
- **Group & Permission System** - Flexible RBAC with granular permissions
- **Visitor Tracking** - Check-in/check-out system with active monitoring
- **JWT Authentication** - Secure token-based auth with BCrypt password hashing
- **Statistics Endpoints** - User counts, group analytics
- **API Documentation** - Interactive Scalar UI

### Web Application
- **Modern Dashboard** - Real-time statistics and analytics
- **User Administration** - Full user lifecycle management
- **Group Management** - Role-based group configuration
- **Visitor Management** - Streamlined check-in/check-out workflow
- **Session Management** - Secure session handling with auto-logout
- **Responsive UI** - Bootstrap 5 with mobile-first design

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 10 |
| **Backend** | ASP.NET Core Web API |
| **Frontend** | ASP.NET Core MVC (Razor Views) |
| **Database** | SQL Server with EF Core 10 (Code-First) |
| **Authentication** | JWT Bearer Tokens |
| **Password Hashing** | BCrypt |
| **Object Mapping** | AutoMapper |
| **UI Framework** | Bootstrap 5 + Bootstrap Icons |
| **API Docs** | Scalar |

## 📁 Project Architecture

```
AccessControl/
├── AccessControl_API/              # Backend REST API
│   ├── Controllers/               # API endpoints
│   │   ├── AuthController.cs     # Login/Register
│   │   ├── UserController.cs     # User CRUD + statistics
│   │   ├── GroupsController.cs   # Group management
│   │   └── VisitLogsController.cs # Visitor check-in/out
│   ├── Services/                 # Business logic layer
│   │   ├── AuthService.cs
│   │   └── IAuthService.cs
│   ├── Data/                     # Database layer
│   │   ├── AppDbContext.cs       # EF Core context
│   │   └── DbSeeder.cs           # Default data seeding
│   ├── Models/                   # Domain entities
│   │   ├── User.cs
│   │   ├── Group.cs
│   │   ├── Permission.cs
│   │   ├── VisitLog.cs
│   │   ├── UserGroup.cs          # Many-to-many
│   │   └── GroupPermission.cs    # Many-to-many
│   ├── Authorization/            # Permission-based auth
│   │   ├── PermissionHandler.cs
│   │   └── PermissionRequirement.cs
│   ├── Utilities/                # Helper classes
│   │   ├── PasswordHasher.cs
│   │   └── JwtTokenGenerator.cs
│   └── Migrations/               # EF Core migrations
│
├── AccessControl.DTO/              # Shared DTOs
│   ├── ApiResponse.cs            # Standardized API responses
│   ├── UserDTO.cs
│   ├── GroupDTO.cs
│   ├── PermissionDTO.cs
│   ├── VisitLogDTO.cs
│   ├── LoginRequestDTO.cs
│   └── RegistrationRequestDTO.cs
│
└── AccessControl_Web/              # Frontend Web App
    ├── Controllers/              # MVC controllers
    │   ├── AuthController.cs     # Login/Register/Logout
    │   ├── HomeController.cs     # Dashboard
    │   ├── UsersController.cs    # User management UI
    │   ├── GroupsController.cs   # Group management UI
    │   └── VisitLogsController.cs # Visitor management UI
    ├── Views/                    # Razor views
    │   ├── Auth/
    │   ├── Home/
    │   ├── Users/
    │   ├── Groups/
    │   ├── VisitLogs/
    │   └── Shared/
    ├── Services/                 # HTTP client services
    │   ├── UserService.cs
    │   ├── GroupService.cs
    │   └── VisitLogService.cs
    ├── Filters/                  # Authorization filters
    │   ├── AuthorizeSessionAttribute.cs
    │   └── RequirePermissionAttribute.cs
    └── wwwroot/                  # Static files (CSS/JS)
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server Express/LocalDB
- Visual Studio 2022 / VS Code / Rider

### Installation Steps

**1. Clone the Repository**
```bash
git clone https://github.com/itumelengseema/AccessControl_API.git
cd AccessControl_API
```

**2. Configure Database Connection**

Edit `appsettings.json` in `AccessControl_API` project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\sqlexpress;Database=AccessControl;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**3. Apply Database Migrations**
```bash
cd AccessControl_API
dotnet ef database update
```

**4. Run the Applications**

**Option A: Visual Studio**
1. Right-click Solution → Properties
2. Select **"Multiple startup projects"**
3. Set both `AccessControl_API` and `AccessControl_Web` to **"Start"**
4. Press **F5**

**Option B: Command Line**
```bash
# Terminal 1 - Start API
cd AccessControl_API
dotnet run --launch-profile http

# Terminal 2 - Start Web App
cd AccessControl_Web
dotnet run
```

**5. Access the Applications**
- 🌐 **Web App**: http://localhost:5208
- 📡 **API Docs**: http://localhost:5000/scalar/v1

### 🔑 Default Login Credentials

The system auto-creates an admin user on first run:

| Field | Value |
|-------|-------|
| **Email** | `admin@access.local` |
| **Password** | `Admin@123` |

> 🔒 **Important**: Change the default password in production!

## 📊 Database Schema

### Entity Relationship Overview

- **Users** ↔ **Groups** (Many-to-Many via `UserGroups`)
- **Groups** ↔ **Permissions** (Many-to-Many via `GroupPermissions`)
- **Users** → **VisitLogs** (One-to-Many)

### Default Seed Data

**Groups:**
- `Admin` - Full system access
- `Security` - Visitor management only

**Permissions:**
- `MANAGE_USERS` - Create/edit/delete users
- `CHECK_IN_VISITOR` - Check in visitors
- `CHECK_OUT_VISITOR` - Check out visitors
- `VIEW_ACTIVE_VISITORS` - View active visitor list

## 🌐 API Reference

### Authentication Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register new user | ❌ |
| `POST` | `/api/auth/login` | Login (returns JWT token) | ❌ |

### User Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/users` | Get all users | ✅ |
| `GET` | `/api/users/count` | Get total user count | ✅ |
| `POST` | `/api/users` | Create new user | ✅ |
| `POST` | `/api/users/{id}` | Update user | ✅ |
| `DELETE` | `/api/users/{id}` | Delete user | ✅ |

### Group Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/groups` | Get all groups | ✅ |
| `GET` | `/api/groups/users-count` | Get user count per group | ✅ |
| `POST` | `/api/groups` | Create new group | ✅ |

### Visit Log Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/vist-logs/active` | Get active visitors | ✅ |
| `POST` | `/api/vist-logs/check-in` | Check in visitor | ✅ |
| `POST` | `/api/vist-logs/check-out/{id}` | Check out visitor | ✅ |

### Sample API Request
```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@access.local","password":"Admin@123"}'

# Get all users (with JWT token)
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🔧 Configuration

### API Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\sqlexpress;Database=AccessControl;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Key": "YourSuperSecretKeyHere_MustBeAtLeast32CharactersLong",
    "Issuer": "AccessControlAPI",
    "Audience": "AccessControlWeb",
    "ExpiryMinutes": 60
  }
}
```

### Web App Configuration (`AccessControl_Web/appsettings.json`)
```json
{
  "ServiceUrls": {
    "AccessControlAPI": "http://localhost:5000"
  },
  "SessionTimeoutMinutes": 30
}
```

## 🧪 Testing

### Using Scalar UI (Interactive API Documentation)
1. Navigate to http://localhost:5000/scalar/v1
2. Explore all available endpoints
3. Test requests directly in the browser
4. View request/response schemas

## 🐛 Troubleshooting

### Cannot Login?
1. **Verify API is running**: Visit http://localhost:5000/scalar/v1
2. **Check default credentials**: `admin@access.local` / `Admin@123`
3. **Check console logs** for error messages

### Database Errors?
```bash
# Reset database completely
dotnet ef database drop --force
dotnet ef database update

# Restart API to trigger seeding
dotnet run --project AccessControl_API.csproj
```

### Port Conflicts?
Update `launchSettings.json` in both projects to use different ports.

### CORS Errors?
Ensure API and Web app are using compatible protocols (both HTTP or both HTTPS).

---

## 🎯 How to Add New Features

This section guides you through extending the AccessControl system with new functionality.

### 📌 **Adding a New Entity (e.g., Department)**

#### **Step 1: Create the Domain Model**
Location: `AccessControl_API/Models/`

```csharp
using System.ComponentModel.DataAnnotations;

namespace AccessControl_API.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public List<User> Users { get; set; } = new();
    }
}
```

#### **Step 2: Create DTOs**
Location: `AccessControl.DTO/`

```csharp
// DepartmentDTO.cs
namespace AccessControl_API.Models.DTO
{
    public class DepartmentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}

// DepartmentCreateUpdateDTO.cs
namespace AccessControl_API.Models.DTO
{
    public class DepartmentCreateUpdateDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
```

#### **Step 3: Update DbContext**
Location: `AccessControl_API/Data/AppDbContext.cs`

```csharp
public DbSet<Department> Departments { get; set; }
```

#### **Step 4: Create Migration**
```bash
dotnet ef migrations add AddDepartmentEntity
dotnet ef database update
```

#### **Step 5: Configure AutoMapper**
Location: `Program.cs` (in AutoMapper configuration section)

```csharp
o.CreateMap<Department, DepartmentDTO>().ReverseMap();
o.CreateMap<DepartmentCreateUpdateDTO, Department>();
```

#### **Step 6: Create API Controller**
Location: `AccessControl_API/Controllers/`

```csharp
using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [Route("api/departments")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public DepartmentsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DepartmentDTO>>>> GetAll()
        {
            var departments = await _db.Departments.ToListAsync();
            var dtos = _mapper.Map<List<DepartmentDTO>>(departments);
            return Ok(ApiResponse<List<DepartmentDTO>>.SuccessResponse(dtos));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentDTO>>> Create(DepartmentCreateUpdateDTO dto)
        {
            var department = _mapper.Map<Department>(dto);
            _db.Departments.Add(department);
            await _db.SaveChangesAsync();
            
            var responseDto = _mapper.Map<DepartmentDTO>(department);
            return Ok(ApiResponse<DepartmentDTO>.CreatedResponse(responseDto));
        }
    }
}
```

#### **Step 7: Create Web Service Interface**
Location: `AccessControl_Web/Services/IServices/`

```csharp
namespace AccessControl_Web.Services.IServices
{
    public interface IDepartmentService
    {
        Task<ApiResponse<List<DepartmentDTO>>?> GetAllDepartmentsAsync();
        Task<ApiResponse<DepartmentDTO>?> CreateDepartmentAsync(DepartmentCreateUpdateDTO dto);
    }
}
```

#### **Step 8: Implement Web Service**
Location: `AccessControl_Web/Services/`

```csharp
using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using System.Net.Http.Headers;

namespace AccessControl_Web.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public DepartmentService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResponse<List<DepartmentDTO>>?> GetAllDepartmentsAsync()
        {
            var client = _httpClientFactory.CreateClient("AccessControlAPI");
            var token = _contextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/departments");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<DepartmentDTO>>>();
            }
            
            return null;
        }

        public async Task<ApiResponse<DepartmentDTO>?> CreateDepartmentAsync(DepartmentCreateUpdateDTO dto)
        {
            var client = _httpClientFactory.CreateClient("AccessControlAPI");
            var token = _contextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.PostAsJsonAsync("/api/departments", dto);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<DepartmentDTO>>();
            }
            
            return null;
        }
    }
}
```

#### **Step 9: Register Service in Program.cs**
Location: `AccessControl_Web/Program.cs`

```csharp
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
```

#### **Step 10: Create Web Controller**
Location: `AccessControl_Web/Controllers/`

```csharp
using AccessControl_Web.Services.IServices;
using AccessControl_Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_Web.Controllers
{
    [AuthorizeSession]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _departmentService.GetAllDepartmentsAsync();
            return View(response?.Data ?? new List<DepartmentDTO>());
        }
    }
}
```

#### **Step 11: Create Views**
Location: `AccessControl_Web/Views/Departments/Index.cshtml`

```razor
@model List<AccessControl_API.Models.DTO.DepartmentDTO>

@{
    ViewData["Title"] = "Departments";
}

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2><i class="bi bi-building"></i> Departments</h2>
        <a asp-action="Create" class="btn btn-primary">
            <i class="bi bi-plus-circle"></i> Add Department
        </a>
    </div>

    <div class="card">
        <div class="card-body">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var dept in Model)
                    {
                        <tr>
                            <td>@dept.Name</td>
                            <td>@dept.Description</td>
                            <td>
                                <a asp-action="Edit" asp-route-id="@dept.Id" class="btn btn-sm btn-warning">
                                    <i class="bi bi-pencil"></i> Edit
                                </a>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>
```

---

### 📌 **Adding a New Permission**

#### **Step 1: Define Permission Constant**
Location: `AccessControl_Web/Helpers/PermissionHelper.cs` (or create it)

```csharp
public static class PermissionHelper
{
    public const string MANAGE_USERS = "MANAGE_USERS";
    public const string CHECK_IN_VISITOR = "CHECK_IN_VISITOR";
    public const string CHECK_OUT_VISITOR = "CHECK_OUT_VISITOR";
    public const string VIEW_ACTIVE_VISITORS = "VIEW_ACTIVE_VISITORS";
    
    // New permission
    public const string MANAGE_DEPARTMENTS = "MANAGE_DEPARTMENTS";
}
```

#### **Step 2: Seed Permission in Database**
Location: `AccessControl_API/Data/DbSeeder.cs`

```csharp
var permissions = new List<Permission>
{
    new Permission { Name = "MANAGE_USERS" },
    new Permission { Name = "CHECK_IN_VISITOR" },
    new Permission { Name = "CHECK_OUT_VISITOR" },
    new Permission { Name = "VIEW_ACTIVE_VISITORS" },
    new Permission { Name = "MANAGE_DEPARTMENTS" } // Add this
};
```

#### **Step 3: Register Policy**
Location: `AccessControl_API/Program.cs`

```csharp
builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "MANAGE_USERS",
        "CHECK_IN_VISITOR",
        "CHECK_OUT_VISITOR",
        "VIEW_ACTIVE_VISITORS",
        "MANAGE_DEPARTMENTS" // Add this
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});
```

#### **Step 4: Use Permission in Controllers**

**API Controller:**
```csharp
[HttpPost]
[Authorize(Policy = "MANAGE_DEPARTMENTS")]
public async Task<IActionResult> CreateDepartment(...)
{
    // Your code
}
```

**Web Controller:**
```csharp
[RequirePermission(PermissionHelper.MANAGE_DEPARTMENTS)]
public async Task<IActionResult> Create()
{
    // Your code
}
```

---

### 📌 **Best Practices for Adding Features**

1. **Follow the Existing Pattern**
   - Use the same folder structure
   - Follow naming conventions (e.g., `EntityNameController`, `IEntityNameService`)
   - Use DTOs for data transfer, not domain models

2. **Always Create Migrations**
   ```bash
   dotnet ef migrations add DescriptiveNameForChange
   dotnet ef database update
   ```

3. **Use AutoMapper for Object Mapping**
   - Never manually map objects in controllers
   - Configure mappings in `Program.cs`

4. **Implement Proper Error Handling**
   - Use try-catch blocks
   - Return appropriate HTTP status codes
   - Use `ApiResponse<T>` for consistent responses

5. **Add Logging**
   ```csharp
   _logger.LogInformation("Action performed: {Details}", details);
   _logger.LogError(ex, "Error occurred: {Message}", ex.Message);
   ```

6. **Test Your Changes**
   - Test API endpoints using Scalar UI
   - Test web pages manually
   - Verify database changes

7. **Update Documentation**
   - Add new endpoints to this README
   - Document any configuration changes
   - Update the architecture diagram if needed

---

## 📝 License

This project was developed as an educational/assessment project.

## 👥 Author

**Itumeleng Seema**
- GitHub: [@itumelengseema](https://github.com/itumelengseema)

---

## ⚡ Quick Commands Reference

```bash
# Development
dotnet run --project AccessControl_API.csproj --launch-profile http
dotnet run --project AccessControl_Web/AccessControl_Web.csproj

# Database
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef database drop --force

# Build & Clean
dotnet clean
dotnet build
dotnet restore

# Run in production mode
dotnet run --configuration Release
```

---

