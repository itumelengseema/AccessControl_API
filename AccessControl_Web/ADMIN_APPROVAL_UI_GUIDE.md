# Admin Approval UI - Implementation Guide

## ? Where to Approve/Decline User Accounts

### ?? **Location in the UI:**

1. **Sidebar Navigation** (for Admin users only):
   ```
   Dashboard
   Users
   ? Pending Approvals  ?? NEW!
   Groups
   Active Visitors
   ```

2. **Dashboard Quick Actions**:
   - Look for the **"Pending Approvals"** card with a pink/purple gradient
   - Located in the "Quick Actions" section

3. **Direct URL**:
   ```
   http://localhost:5208/Users/PendingApprovals
   ```

---

## ?? **How to Use:**

### Step 1: Login as Admin
- Email: `admin@access.local`
- Password: `Admin@123`

### Step 2: Navigate to Pending Approvals
Choose one of:
- Click **"Pending Approvals"** in the sidebar (left menu)
- Click the **"Pending Approvals"** card on the Dashboard
- Go directly to `/Users/PendingApprovals`

### Step 3: Review Pending Users
You'll see cards showing:
- **User Name**
- **Email**
- **ID Number**  
- **Role** (Admin or Security)
- **Status Badge** (Pending)

### Step 4: Take Action
Each user card has two buttons:

#### ? **Approve Button** (Green)
- Sets `IsApproved = true`
- User can now login
- User receives full permissions for their role

#### ? **Reject Button** (Red)
- **Permanently deletes** the user registration
- Cannot be undone
- User will need to re-register

---

## ?? **Security & Permissions**

### Who Can Access This Page?
? **Admin users only** (requires `MANAGE_USERS` permission)

? Regular users and Security guards **cannot** access this page

### What Happens When Users Register?

| Group | Auto-Approved? | Needs Admin Review? |
|-------|----------------|---------------------|
| **Visitor** | ? Yes | ? No |
| **Security** | ? No | ? Yes |
| **Admin** | ? No | ? Yes |

---

## ?? **UI Features:**

### Empty State
When no pending approvals:
- Shows a success icon
- Message: "No Pending Approvals"
- "All user registrations have been reviewed"

### With Pending Users
- Shows count: "X user registrations awaiting review"
- Cards with user information
- **Approve** (green) and **Reject** (red) buttons
- Confirmation dialogs before approval/rejection

### Success/Error Messages
- Green alert: "User has been approved successfully!"
- Red alert if something fails
- Auto-dismiss after 5 seconds

---

## ??? **Files Created/Modified:**

### Backend (API):
- ? Already done - endpoints exist

### Frontend (Web UI):
1. **`AccessControl_Web/Services/IServices/IUserService.cs`**
   - Added: `GetPendingApprovalsAsync()`
   - Added: `ApproveUserAsync(int userId)`
   - Added: `RejectUserAsync(int userId)`

2. **`AccessControl_Web/Services/UserService.cs`**
   - Implemented the 3 new methods

3. **`AccessControl_Web/Controllers/UsersController.cs`**
   - Added: `PendingApprovals()` action
   - Added: `Approve(int id)` POST action
   - Added: `Reject(int id)` POST action

4. **`AccessControl_Web/Views/Users/PendingApprovals.cshtml`**
   - **NEW FILE** - The approval UI page

5. **`AccessControl_Web/Views/Shared/_Layout.cshtml`**
   - Added "Pending Approvals" menu item (Admin only)

6. **`AccessControl_Web/Views/Home/Index.cshtml`**
   - Added "Pending Approvals" quick action card

---

## ?? **Testing the Approval Workflow:**

### Test Scenario:

1. **Register a Security User**:
   ```
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

2. **Try to Login** (should fail):
   ```
   POST /api/auth/login
   {
     "email": "jane@security.com",
     "password": "SecurePass123"
   }
   ```
   Expected: **403 Forbidden** with message: "Your account is pending approval..."

3. **Login as Admin**:
   - Go to http://localhost:5208/Users/PendingApprovals

4. **See Jane's Account** in pending list

5. **Click "Approve"**

6. **Now Jane Can Login** successfully! ?

---

## ?? **UI Design:**

- **Modern card-based layout**
- **Color-coded badges**:
  - ?? Yellow "Pending" badge
  - ?? Red "Admin" badge
  - ?? Blue "Security" badge
- **Gradient action cards** for quick access
- **Hover effects** on cards
- **Responsive design** for mobile/tablet
- **Confirmation dialogs** before actions

---

## ?? **Next Steps:**

1. **Run the Web App**:
   ```
   dotnet run --project AccessControl_Web
   ```

2. **Login as Admin**

3. **Navigate to Pending Approvals**

4. **Start Approving Users!**

---

## ?? **Pro Tips:**

- **Bookmark** the Pending Approvals page for quick access
- Check it daily if you expect new Admin/Security registrations
- **Be careful with Reject** - it permanently deletes the registration
- Users get a clear message when login fails due to pending approval

---

## ?? **Need Help?**

If you don't see "Pending Approvals" in your menu:
1. Make sure you're logged in as **Admin**
2. Check you have the **MANAGE_USERS** permission
3. Clear browser cache and refresh

Enjoy your new approval workflow! ??
