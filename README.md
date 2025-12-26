# AccessControl API

A Building Access Control System RESTful API built with ASP.NET Core 10 for managing users, groups, and permissions in a role-based access control (RBAC) architecture.

## Description

This API provides a comprehensive solution for managing building access control through a hierarchical permission system. Users are assigned to groups, and groups are granted specific permissions, enabling flexible and scalable access management. The system is designed to efficiently check whether a user has the necessary permissions to access specific resources or areas.

## Features

- **User Management**: Create and retrieve users with personal information
- **Group Management**: Organize users into logical groups (e.g., Admin, Security, Visitor)
- **Permission Management**: Define granular access permissions (e.g., Door Access, Camera Access)
- **Access Verification**: Real-time permission checking for users
- **Many-to-Many Relationships**: Users can belong to multiple groups; groups can have multiple permissions
- **Entity Framework Core**: Code-first database approach with migrations
- **RESTful API Design**: Clean, intuitive endpoint structure
- **Interactive API Documentation**: Integrated Scalar UI for testing

## Technology Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **SQL Server Express**
- **Scalar API Documentation**
- **C# 14**

## Database Design

The system uses five main entities with the following structure:

### Entities

#### Users
- `Id` (Guid, PK)
- `Name` (string, required)
- `Surname` (string, required)
- `Email` (string, optional)
- `PhoneNumber` (string, required)

#### Groups
- `Id` (Guid, PK)
- `Name` (string, required)

#### Permissions
- `Id` (Guid, PK)
- `Name` (string, required)

#### UserGroups (Junction Table)
- `UserId` (Guid, PK, FK)
- `GroupId` (Guid, PK, FK)

#### GroupPermissions (Junction Table)
- `GroupId` (Guid, PK, FK)
- `PermissionId` (Guid, PK, FK)

### Entity Relationships

```
User ←→ UserGroup ←→ Group ←→ GroupPermission ←→ Permission
```

- A User can belong to multiple Groups
- A Group can have multiple Users
- A Group can have multiple Permissions
- A Permission can be assigned to multiple Groups

## API Endpoints

### Users

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| POST | `/api/users` | Create a new user | User object |
| GET | `/api/users` | Get all users | - |

### Groups

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| POST | `/api/groups` | Create a new group | Group object |
| GET | `/api/groups` | Get all groups | - |

### Permissions

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| POST | `/api/permissions` | Create a new permission | Permission object |
| GET | `/api/permissions` | Get all permissions | - |

### Access Control

| Method | Endpoint | Description | Query Parameters |
|--------|----------|-------------|------------------|
| GET | `/api/access/check` | Check if a user has a specific permission | `userId` (Guid), `permission` (string) |

### Sample Request Examples

**Create User:**
```json
POST /api/users
{
  "name": "John",
  "surname": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890"
}
```

**Check Access:**
```
GET /api/access/check?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6&permission=Door%20Access
```

## Seed Data

The system can be initialized with default groups and permissions:

### Default Groups
- **Admin**: Full system access
- **Security**: Access to security features
- **Employee**: Standard building access
- **Visitor**: Limited guest access

### Default Permissions
- **Door Access**: Physical door entry
- **Camera Access**: Security camera viewing
- **Admin Panel**: Administrative functions
- **Parking Access**: Parking facility entry
- **Conference Room**: Meeting room booking

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server Express or SQL Server LocalDB
- Git (for cloning the repository)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/itumelengseema/AccessControl_API.git
   cd AccessControl_API
   ```

2. **Update the connection string:**
   
   Open `appsettings.json` and verify the connection string matches your SQL Server instance:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER\\sqlexpress;Database=AccessControl;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Access the API:**
   - API Base URL: `https://localhost:5001` or `http://localhost:5000`
   - Interactive Documentation: `https://localhost:5001/scalar/v1` (when running in Development mode)

## Usage

### Using Scalar UI (Recommended)

1. Navigate to `https://localhost:5001/scalar/v1` in your browser
2. Explore available endpoints in the interactive documentation
3. Test endpoints directly from the UI with sample requests

### Using cURL

**Create a user:**
```bash
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane",
    "surname": "Smith",
    "email": "jane.smith@example.com",
    "phoneNumber": "+1234567890"
  }'
```

**Check access:**
```bash
curl -X GET "https://localhost:5001/api/access/check?userId=<USER_ID>&permission=Door%20Access"
```

### Using Postman

1. Import the API by using the base URL: `https://localhost:5001`
2. Create requests for each endpoint
3. Use JSON format for POST request bodies

## Project Structure

```
AccessControl_API/
├── Controllers/
│   ├── UserController.cs
│   ├── AccessControll.cs
│   └── [Additional Controllers]
├── Models/
│   ├── User.cs
│   ├── Group.cs
│   ├── Permission.cs
│   ├── UserGroup.cs
│   └── GroupPermission.cs
├── Data/
│   └── AppDbContext.cs
├── Migrations/
│   └── [EF Core Migrations]
├── appsettings.json
├── Program.cs
└── AccessControl_API.csproj
```

## Notes

### Scope and Limitations

This project was developed as an **intern-level assessment** and focuses on core functionality:

- ✅ **Implemented**: CRUD operations, relationship mapping, access checking
- ⚠️ **Not Implemented**: 
  - Authentication/Authorization (JWT, Identity)
  - Input validation and error handling
  - Logging and monitoring
  - Unit and integration tests
  - Data seeding on startup
  - Pagination for list endpoints
  - Soft delete functionality
  - Audit trails

### Development Context

This API demonstrates fundamental understanding of:
- RESTful API design principles
- Entity Framework Core and database relationships
- ASP.NET Core dependency injection
- Many-to-many relationship implementation
- Code-first database migrations

## Future Enhancements

- Implement JWT authentication
- Add comprehensive input validation using FluentValidation
- Implement repository pattern and unit of work
- Add logging with Serilog
- Create unit and integration tests
- Implement CQRS pattern with MediatR
- Add API versioning
- Implement caching strategies

## Contributing

This is an assessment project. For feedback or suggestions, please contact the repository owner.

## License

This project is developed for educational and assessment purposes.

## Author

**Itumeleng Seema**
- GitHub: [@itumelengseema](https://github.com/itumelengseema)

