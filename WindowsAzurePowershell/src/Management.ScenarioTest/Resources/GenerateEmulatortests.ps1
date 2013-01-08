Import-Module azure
.".\Emulator.ps1"
Run-Test {Test-NodeServiceCreation} ".\emtest001.log" -generate
Run-Test {Test-PHPServiceCreation} ".\emtest002.log" -generate
Write-Host
Write-Host -ForegroundColor Green "All Baseline files generated"
Write-Host
