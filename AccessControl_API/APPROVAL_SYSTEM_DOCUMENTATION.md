# User Account Approval System

## Overview
Implemented a user account approval system where users registering as **Admin** or **Security** require approval from the default admin before they can login.

## Changes Made

### 1. **Database Schema Updates**

#### User Model (`AccessControl_API/Models/User.cs`)
Added three new fields:
```csharp
[Required]
public bool IsApproved { get; set; } = false;

public DateTime? ApprovedAt { get; set; }

public int? ApprovedBy { get; set; }
```

- `IsApproved`: Indicates if the user account is approved
- `ApprovedAt`: Timestamp when the user was approved
- `ApprovedBy`: ID of the admin who approved the account (for audit trail)

**Migration**: Run `dotnet ef database update` to apply the `AddUserApprovalFields` migration

### 2. **Registration Logic (`AuthService.cs`)**

#### Auto-Approval vs Manual Approval
```csharp
// Determine if approval is needed based on group
bool requiresApproval = group.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                       group.Name.Equals("Security", StringComparison.OrdinalIgnoreCase);

// Auto-approve if not Admin/Security
IsApproved = !requiresApproval
```

**Behavior:**
- **Regular users** (non-Admin/Security): Auto-approved (`IsApproved = true`)
- **Admin/Security users**: Requires approval (`IsApproved = false`)

### 3. **Login Logic (`AuthService.cs`)**

Added approval check:
```csharp
// Check if user account is approved
if (!user.IsApproved)
{
    return null; // Account not approved yet
}
```

**Behavior:**
- Users with `IsApproved = false` cannot login
- Returns `null` (same as invalid credentials for security)

### 4. **Default Admin User (`DbSeeder.cs`)**

Updated to be pre-approved:
```csharp
var adminUser = new User
{
    ...
    IsApproved = true, // Default admin is pre-approved
    ApprovedAt = DateTime.UtcNow
};
```

**Default Credentials:**
- Email: `admin@access.local`
- Password: `Admin@123`

### 5. **New API Endpoints (`UserController.cs`)**

#### GET `/api/users/pending-approvals`
Returns all users awaiting approval.

**Response:**
```json
{
  "success": true,
  "status": 200,
  "message": "Pending approvals retrieved successfully",
  "data": [
    {
      "id": 2,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "groupName": "Security"
    }
  ]
}
```

#### POST `/api/users/{id}/approve`
Approves a pending user account.

**Response:**
```json
{
  "success": true,
  "status": 200,
  "message": "User approved successfully",
  "data": { ...userDTO... }
}
```

#### POST `/api/users/{id}/reject`
Rejects and deletes a pending user registration.

**Response:**
```json
{
  "success": true,
  "status": 200,
  "message": "User registration rejected and removed"
}
```

## User Workflow

### Registration Flow

1. **User registers** choosing a group (Regular/Security/Admin)
2. **System checks** group type:
   - **Regular Group**: User is auto-approved ? Can login immediately
   - **Admin/Security**: User account created but `IsApproved = false`
3. **If Admin/Security**: User receives registration confirmation but cannot login yet

### Admin Approval Flow

1. **Admin logs in** with default credentials
2. **Admin views** pending approvals: `GET /api/users/pending-approvals`
3. **Admin reviews** user details (name, email, group)
4. **Admin decides**:
   - **Approve**: `POST /api/users/{id}/approve` ? User can now login
   - **Reject**: `POST /api/users/{id}/reject` ? Registration removed

### Login Flow

1. **User attempts** to login
2. **System checks**:
   - ? Email exists?
   - ? Password correct?
   - ? Account approved? ? **NEW CHECK**
3. **If not approved**: Login fails (returns null)
4. **If approved**: Login succeeds, returns JWT token

## Testing Considerations

### Test Updates Needed

1. **Existing Tests**: 
   - All existing User creation tests need `IsApproved = true` or `PasswordHash = "DummyHash"`
   - Login tests for Admin/Security should handle unapproved scenarios

2. **New Tests to Add**:
   ```csharp
   - RegisterAsync_SecurityGroup_CreatesUnapprovedUser()
   - RegisterAsync_AdminGroup_CreatesUnapprovedUser()
   - RegisterAsync_RegularGroup_CreatesApprovedUser()
   - LoginAsync_UnapprovedUser_ReturnsNull()
   - LoginAsync_ApprovedUser_ReturnsToken()
   - ApproveUser_ValidId_ApprovesSuccessfully()
   - RejectUser_ValidId_RemovesUser()
   - GetPendingApprovals_ReturnsOnlyUnapprovedUsers()
   ```

## Security Notes

1. **Default Admin**: The seeded admin account is pre-approved to prevent lockout
2. **Approval Required**: Only Admin and Security groups require approval
3. **Password Security**: Unapproved users' passwords are still hashed
4. **Audit Trail**: `ApprovedBy` and `ApprovedAt` track who approved and when

## Next Steps

1. ? Run database migration: `dotnet ef database update`
2. ? Update existing tests with approval logic
3. ? Add new tests for approval workflows
4. ? Add authorization attributes to approval endpoints (require MANAGE_USERS permission)
5. ? Update Web UI to show approval status and provide admin approval interface
6. ? Add notifications (email/dashboard) when users await approval

## Example Usage

### Register Security User (Requires Approval)
```http
POST /api/auth/register
{
  "firstName": "Jane",
  "lastName": "Security",
  "email": "jane@security.com",
  "password": "SecurePass123",
  "identificationNumber": "SEC001",
  "groupId": 2  // Security group
}
```
? User created with `IsApproved = false`

### Login (Will Fail Until Approved)
```http
POST /api/auth/login
{
  "email": "jane@security.com",
  "password": "SecurePass123"
}
```
? Returns `null` (unauthorized) until admin approves

### Admin Approves User
```http
POST /api/users/2/approve
Authorization: Bearer {admin-jwt-token}
```
? Sets `IsApproved = true`, `ApprovedAt = DateTime.UtcNow`

### Login Again (Now Succeeds)
```http
POST /api/auth/login
{
  "email": "jane@security.com",
  "password": "SecurePass123"
}
```
? Returns JWT token and user data
