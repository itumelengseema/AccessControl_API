# AccessControl System

A complete **Building Access Control System** with RESTful API and Web Application built with **ASP.NET Core (.NET 10)** for managing users, groups, permissions, and visitor access in a role-based access control (RBAC) architecture.

## 🌟 Features

### API Backend
- **User Management** - Create, read, update, delete users with authentication
- **Group Management** - Organize users into logical groups (Admin, Security, Employee, Visitor)
- **Permission System** - Granular permissions with group-based access control
- **Visitor Tracking** - Check-in/check-out system with active visitor monitoring
- **JWT Authentication** - Secure token-based authentication
- **AutoMapper** - Clean object mapping between DTOs and entities
- **Interactive API Docs** - Scalar UI for testing endpoints

### Web Application
- **Authentication** - Login/Register with session management
- **Dashboard** - Real-time statistics and quick actions
- **User CRUD** - Complete user management interface
- **Group Management** - Create and manage user groups
- **Visitor Check-In/Out** - Track active visitors with duration monitoring
- **Responsive Design** - Bootstrap 5 with mobile support

## 🛠️ Technology Stack

- **.NET 10**
- **ASP.NET Core MVC / Razor Pages**
- **Entity Framework Core 10**
- **SQL Server Express**
- **JWT Bearer Authentication**
- **AutoMapper**
- **BCrypt for Password Hashing**
- **Bootstrap 5 + Bootstrap Icons**
- **Scalar API Documentation**

## 📁 Project Structure

```
AccessControl/
├── AccessControl_API/          # Backend API
│   ├── Controllers/           # API endpoints
│   ├── Services/             # Business logic
│   ├── Data/                 # EF Core DbContext & Seeder
│   ├── Models/               # Domain entities
│   ├── Authorization/        # Permission handlers
│   └── Utilities/           # Password hashing, JWT generation
│
├── AccessControl.DTO/         # Shared Data Transfer Objects
│   ├── ApiResponse.cs
│   ├── UserDTO.cs
│   ├── GroupDTO.cs
│   ├── VisitLogDTO.cs
│   └── AuthDTOs.cs
│
└── AccessControl_Web/         # Frontend Web App
    ├── Controllers/          # MVC controllers
    ├── Views/               # Razor views
    ├── Services/            # HTTP client services
    └── Filters/            # Session authorization
```

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server Express or LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/itumelengseema/AccessControl_API.git
   cd AccessControl_API
   ```

2. **Configure Database**
   
   Update `appsettings.json` in the API project:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\sqlexpress;Database=AccessControl;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Start Both Projects**

   **Option A: Visual Studio**
   - Right-click Solution → Set Startup Projects
   - Select "Multiple startup projects"
   - Set both `AccessControl_API` and `AccessControl_Web` to "Start"
   - Press F5

   **Option B: Command Line**
   ```bash
   # Terminal 1 - API
   dotnet run --project AccessControl_API.csproj --launch-profile http

   # Terminal 2 - Web
   cd AccessControl_Web
   dotnet run
   ```

5. **Access the Applications**
   - **API**: http://localhost:5000/scalar/v1
   - **Web App**: http://localhost:5208

## 🔑 Default Credentials

The system creates a default admin user on first run:

```
Email:    admin@access.local
Password: Admin@123
```

**Login at:** http://localhost:5208/Auth/Login

## 📊 Database Schema

### Core Entities

- **Users** - User accounts with authentication
- **Groups** - Organizational groups (Admin, Security, etc.)
- **Permissions** - Access permissions
- **VisitLogs** - Check-in/check-out records
- **UserGroups** - Many-to-many: Users ↔ Groups
- **GroupPermissions** - Many-to-many: Groups ↔ Permissions

### Default Data

**Groups:**
- Admin (full access)
- Security (limited access)

**Permissions:**
- MANAGE_USERS
- CHECK_IN_VISITOR
- CHECK_OUT_VISITOR
- VIEW_ACTIVE_VISITORS

## 🌐 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login (returns JWT) |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/count` | Get user count |
| POST | `/api/users` | Create user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |

### Groups
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/groups` | Get all groups |
| GET | `/api/groups/users-count` | Users per group |
| POST | `/api/groups` | Create group |

### Visit Logs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/vist-logs/active` | Get active visitors |
| POST | `/api/vist-logs/check-in` | Check in visitor |
| POST | `/api/vist-logs/check-out/{id}` | Check out visitor |

## 🎨 Web Application Features

### Dashboard
- Total users count
- Active visitors count
- Groups count
- Quick action buttons

### User Management
- List all users
- Create new users
- Edit user details
- Delete users

### Group Management
- View all groups
- Create new groups
- View group statistics
- User distribution charts

### Visitor Tracking
- Active visitors list
- Check-in form with car registration
- One-click check-out
- Duration tracking
- Real-time statistics

## 🔧 Configuration

### API Settings (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\sqlexpress;Database=AccessControl;..."
  },
  "JwtSettings": {
    "Key": "YourSuperSecretKeyHere",
    "Issuer": "AccessControlAPI",
    "Audience": "AccessControlWeb"
  }
}
```

### Web App Settings (`AccessControl_Web/appsettings.json`)
```json
{
  "ServiceUrls": {
    "AccessControlAPI": "http://localhost:5000"
  }
}
```

## 🧪 Testing

### Using Scalar UI
1. Navigate to http://localhost:5000/scalar/v1
2. Test endpoints interactively
3. View request/response examples

### Sample API Calls

**Register:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "identificationNumber": "123456",
    "password": "Test123!",
    "groupId": 1
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@access.local",
    "password": "Admin@123"
  }'
```

## 📝 Important Notes

### Session Management
- 30-minute session timeout
- Token stored in session
- Automatic logout on expiry

### Security
- Passwords hashed with BCrypt
- JWT token authentication
- Session-based authorization
- CSRF protection on forms

### CORS Configuration
Configured to allow requests from:
- https://localhost:7154
- http://localhost:5154
- http://localhost:5000

## 🐛 Troubleshooting

### Can't Login?
1. **Verify API is running:** http://localhost:5000/scalar/v1
2. **Check credentials:** `admin@access.local` / `Admin@123`
3. **View logs:** Check terminal for error messages

### Database Issues?
```bash
# Reset database
dotnet ef database drop --force
dotnet ef database update

# Restart API to reseed
dotnet run --project AccessControl_API.csproj
```

### SSL Errors?
- API and Web must use same protocol (both HTTP or both HTTPS)
- For HTTP: Use `--launch-profile http` when starting API
- For HTTPS: Run `dotnet dev-certs https --trust`

## 📚 Additional Documentation

- **[Quick Start Guide](QUICK_START_GUIDE.md)** - Detailed setup instructions
- **[Project Status](PROJECT_STATUS.md)** - Current implementation status
- **[Default Credentials](DEFAULT_LOGIN_CREDENTIALS.md)** - Admin login info

## 🎯 Future Enhancements

- [ ] Email verification
- [ ] Password reset functionality
- [ ] User profile management
- [ ] Advanced search and filtering
- [ ] Report generation (PDF/Excel)
- [ ] Real-time notifications
- [ ] Audit logging
- [ ] Role hierarchy
- [ ] API rate limiting
- [ ] Dockerization

## 👥 Contributing

This project was developed as an educational/assessment project. For suggestions:
1. Fork the repository
2. Create a feature branch
3. Submit a pull request




---

## ⚡ Quick Commands Reference

```bash
# Start API (HTTP)
dotnet run --project AccessControl_API.csproj --launch-profile http

# Start Web App
cd AccessControl_Web && dotnet run

# Database migrations
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef database drop --force

# Build & clean
dotnet clean
dotnet build
dotnet restore
```

---
