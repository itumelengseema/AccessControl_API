# Quick Login Test

## Default Admin Credentials

**CORRECT Email:** `admin@access.local`  
**WRONG Email:** ? `admin@admin.local`

**Password:** `Admin@123`

## Test the Login

### Option 1: Using Browser (Easiest)
1. Make sure your API is running
2. Open: http://localhost:5000/scalar/v1
3. Find `POST /api/auth/login`
4. Click "Try it out"
5. Paste this JSON:
```json
{
  "email": "admin@access.local",
  "password": "Admin@123"
}
```
6. Click "Execute"

### Option 2: Using PowerShell
```powershell
# Login request
$body = @{
    email = "admin@access.local"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Show the result
$response | ConvertTo-Json -Depth 10
```

### Expected Response (Success)
```json
{
  "success": true,
  "status": 200,
  "message": "Login successful.",
  "data": {
    "result": 0,
    "message": "Login successful.",
    "user": {
      "id": 1,
      "firstName": "System",
      "lastName": "Admin",
      "email": "admin@access.local",
      "identificationNumber": "ADMIN-001",
      "groupId": 1,
      "groupName": "Admin"
    },
    "token": "eyJhbGc...",
    "permissions": [
      "MANAGE_USERS",
      "CHECK_IN_VISITOR",
      "CHECK_OUT_VISITOR",
      "VIEW_ACTIVE_VISITORS"
    ]
  },
  "timeStamp": "2026-01-15T..."
}
```

### If Login Fails

**Error: "Invalid email or password"**
- Make sure you're using `admin@access.local` (NOT `admin@admin.local`)
- Password is case-sensitive: `Admin@123`

**Error: "Account pending approval"**
- This shouldn't happen for default admin, but if it does:
  - The database wasn't seeded properly
  - Run: `dotnet ef database drop --project AccessControl_API --force`
  - Run: `dotnet ef database update --project AccessControl_API`
  - Restart the API

**Error: Connection refused**
- Make sure API is running: `dotnet run --project AccessControl_API`
- Check it's on port 5000: `http://localhost:5000`

## Troubleshooting: Re-seed Database

If nothing works, reset the database:

```powershell
# Stop the API first (Ctrl+C)

# Drop database
dotnet ef database drop --project AccessControl_API --force

# Apply migrations (this will also seed)
dotnet ef database update --project AccessControl_API

# Run API again
dotnet run --project AccessControl_API
```

The seeding happens automatically when the API starts if the database is empty.
