# Reset Database Script
# Run this if you can't login with default admin

Write-Host "?? Resetting Database..." -ForegroundColor Yellow

# Stop any running API
Get-Process -Name "AccessControl_API" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

# Drop and recreate database
Set-Location "AccessControl_API"

Write-Host "?? Dropping database..." -ForegroundColor Cyan
dotnet ef database drop --force

Write-Host "?? Applying migrations..." -ForegroundColor Cyan
dotnet ef database update

Write-Host "? Database reset complete!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Default Admin Credentials:" -ForegroundColor Yellow
Write-Host "   Email: admin@access.local" -ForegroundColor White
Write-Host "   Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "?? Starting API..." -ForegroundColor Cyan
dotnet run
