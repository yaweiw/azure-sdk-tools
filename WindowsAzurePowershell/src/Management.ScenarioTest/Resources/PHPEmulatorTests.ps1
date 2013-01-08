Import-Module azure
.".\Emulator.ps1"
$global:totalCount = 0;
$global:passedCount = 0;

function Run-TestProtected
{
   param([ScriptBlock]$script, [string] $testName)
   try 
   {
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host  -ForegroundColor Green "Running test $testName"
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host
     &$script
	 $global:passedCount = $global:passedCount + 1
	 Write-Host
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host -ForegroundColor Green "Test Passed"
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host
   }
   catch
   {
     Out-String -InputObject $_.Exception | Write-Host -ForegroundColor Red
	 Write-Host
     Write-Host  -ForegroundColor Red =====================================
	 Write-Host -ForegroundColor Red "Test Failed"
     Write-Host  -ForegroundColor Red =====================================
	 Write-Host
   }
   finally
   {
      $global:totalCount = $global:totalCount + 1
   }
}
Run-TestProtected {Run-Test {Test-PHPServiceCreation} ".\emtest002.log"} "Emulator PHP Hello World Scenario"
Run-TestProtected {Run-Test {Test-PHPHelloInEmulator}} "Emulator PHP Hello World Scenario"
Write-Host
Write-Host -ForegroundColor Green "$global:passedCount / $global:totalCount Emulator Tests Pass"
Write-Host

