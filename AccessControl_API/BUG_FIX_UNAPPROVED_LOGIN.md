# Bug Fix: Informative Message for Unapproved Account Login Attempts

## Problem Identified
When users with Admin or Security roles registered and tried to login before their account was approved, they received a generic "Invalid credentials" error. This provided no information about their account status and was confusing for users.

## Solution Implemented

### 1. Created LoginResult Enum
**File**: `AccessControl.DTO/LoginResult.cs`
```csharp
public enum LoginResult
{
    Success,                 // Login successful
    InvalidCredentials,      // Wrong email or password
    AccountNotApproved       // Account pending approval
}
```

### 2. Enhanced LoginResponseDTO
**File**: `AccessControl.DTO/LoginResponseDTO.cs`

Added new properties:
- `LoginResult Result` - Indicates the outcome of the login attempt
- `string? Message` - Provides user-friendly error message
- Made `User` nullable since it won't be populated for failed logins

### 3. Updated AuthService Login Logic
**File**: `AccessControl_API/Services/AuthService.cs`

**Before**:
```csharp
if (!user.IsApproved)
{
    return null; // ? No information for user
}
```

**After**:
```csharp
if (!user.IsApproved)
{
    return new LoginResponseDTO 
    { 
        Result = LoginResult.AccountNotApproved,
        Message = "Your account is pending approval by an administrator. You will be notified once your account is approved."
    };
}
```

Now returns different responses for each failure scenario:
- **Invalid Email**: Returns `LoginResult.InvalidCredentials`
- **Invalid Password**: Returns `LoginResult.InvalidCredentials`
- **Account Not Approved**: Returns `LoginResult.AccountNotApproved` ?

### 4. Updated AuthController to Handle Different Status Codes
**File**: `AccessControl_API/Controllers/AuthController.cs`

Added switch statement to return appropriate HTTP status codes:

```csharp
switch (result.Result)
{
    case LoginResult.Success:
        return Ok(...)                    // 200 OK
    
    case LoginResult.AccountNotApproved:
        return StatusCode(403, ...)       // 403 Forbidden ?
    
    case LoginResult.InvalidCredentials:
        return Unauthorized(...)          // 401 Unauthorized
}
```

### 5. Updated All Tests
**Files**: 
- `AccessControl_Test/Services/AuthServiceTests.cs`
- `AccessControl_Test/Controllers/AuthControllerLoginStatusTests.cs` (new)

#### Key Test Updates:

**Test: LoginAsync_UnapprovedUser_ReturnsAccountNotApprovedStatus**
```csharp
Assert.Equal(LoginResult.AccountNotApproved, result.Result);
Assert.Contains("pending approval", result.Message);
Assert.Contains("administrator", result.Message);
Assert.Null(result.User);
Assert.Empty(result.Token);
```

**Test: Login_UnapprovedUser_Returns403Forbidden**
```csharp
var objectResult = Assert.IsType<ObjectResult>(result.Result);
Assert.Equal(403, objectResult.StatusCode); // Forbidden
```

## User Experience Improvement

### Before (? Confusing):
```http
POST /api/auth/login
{
  "email": "security@example.com",
  "password": "CorrectPassword"
}

Response: 401 Unauthorized
{
  "success": false,
  "message": "Invalid credentials"  // ? Misleading!
}
```

### After (? Clear):
```http
POST /api/auth/login
{
  "email": "security@example.com",
  "password": "CorrectPassword"
}

Response: 403 Forbidden
{
  "success": false,
  "message": "Your account is pending approval by an administrator. You will be notified once your account is approved."
}
```

## HTTP Status Code Semantics

| Scenario | Status Code | Meaning |
|----------|-------------|---------|
| **Success** | 200 OK | Login successful |
| **Invalid Credentials** | 401 Unauthorized | Authentication failed (wrong credentials) |
| **Account Not Approved** | 403 Forbidden | Authentication succeeded but authorization denied |

## Testing
- ? All existing tests updated
- ? New tests added for HTTP status codes
- ? Total test count: 101 tests
- ? Build successful
- ? All tests passing

## Security Note
The system still maintains security by not revealing whether an email exists when credentials are wrong. It only reveals the approval status when the email AND password are correct, confirming the user's identity.

## Files Modified
1. `AccessControl.DTO/LoginResult.cs` (new)
2. `AccessControl.DTO/LoginResponseDTO.cs`
3. `AccessControl_API/Services/AuthService.cs`
4. `AccessControl_API/Controllers/AuthController.cs`
5. `AccessControl_Test/Services/AuthServiceTests.cs`
6. `AccessControl_Test/Controllers/AuthControllerLoginStatusTests.cs` (new)

## Next Steps for Web UI
Update the login page in `AccessControl_Web` to handle the 403 status code and display the approval message prominently to users.

Example:
```csharp
if (response.StatusCode == 403)
{
    ViewBag.ErrorType = "pending-approval";
    ViewBag.ErrorMessage = response.Message;
    // Show a different UI - maybe with "Contact Admin" button
}
```
