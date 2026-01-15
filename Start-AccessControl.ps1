# Start-AccessControl.ps1
# Runs both API and Web app in separate windows

Write-Host "`n?? Starting Access Control System..." -ForegroundColor Green
Write-Host "???????????????????????????????????????????????`n" -ForegroundColor Cyan

# Start API
Write-Host "?? Starting API Server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host '?? Access Control API' -ForegroundColor Green; Write-Host '???????????????????????' -ForegroundColor Cyan; cd AccessControl_API; dotnet run" -WindowStyle Normal

Start-Sleep -Seconds 3

# Start Web
Write-Host "?? Starting Web Application..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host '?? Access Control Web UI' -ForegroundColor Green; Write-Host '????????????????????????' -ForegroundColor Cyan; cd AccessControl_Web; dotnet run" -WindowStyle Normal

Start-Sleep -Seconds 5

Write-Host "`n? Applications starting...`n" -ForegroundColor Green
Write-Host "?? API:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "?? API Docs: http://localhost:5000/scalar/v1" -ForegroundColor Cyan
Write-Host "?? Web:  http://localhost:5208" -ForegroundColor Cyan
Write-Host "`n?? Admin Login:" -ForegroundColor Yellow
Write-Host "   Email:    admin@access.local" -ForegroundColor White
Write-Host "   Password: Admin@123" -ForegroundColor White
Write-Host "`n?? Tip: Pending Approvals page is in the sidebar!" -ForegroundColor Magenta
Write-Host "????????????????????????????????????????????????`n" -ForegroundColor Cyan

# Open browser
Start-Sleep -Seconds 8
Start-Process "http://localhost:5208"
