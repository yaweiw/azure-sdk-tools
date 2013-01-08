Import-Module Azure
.".\Credentials.ps1"
Run-Test {Test-ImportPublishSettingsFile ".\tipsf001.publishsettings"} .\tipsf001.log -generate
Run-Test {Test-ImportPublishSettingsFile ".\tipsf002.publishsettings"} .\tipsf002.log -generate
Run-Test {Test-ImportPublishSettingsFile ".\tipsf003.publishsettings"} .\tipsf003.log -generate
Write-Host
Write-Host -ForegroundColor Green "All Baseline files generated"
Write-Host
