$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupEnv.ps1')

Import-Module "$env:AzurePSRoot\src\Package\Debug\ServiceManagement\azure\Azure.psd1"
cd  "$env:AzurePSRoot\src\Package\Debug"

Stop-AzureEmulator

Write-Host "Testing Caching role with MemCacheShim package, Node Web Role, and run under emulators" -ForegroundColor "Green"
#detect nodejs for x86 is installed, if not install it

if (!(Test-Path "$env:ADXSDKProgramFiles\iisnode-dev")) {
    Write-Host "You must install Node.js/32-bits at http://nodejs.org/download/ and then install azure node.js support from http://azure.microsoft.com/en-us/downloads/" -ForegroundColor "Red"
	Exit
}

# create testing folder
cd "$env:AzurePSRoot\Package\Debug"
$testFolder = "$env:AzurePSRoot\src\Package\Debug\SDKTest"
if (Test-Path $testFolder){
   Remove-Item $testFolder -Recurse -Force
}
md $testFolder
cd $testFolder 

New-AzureServiceProject MemCacheTestWithNode
Add-AzureNodeWebRole WebRole1
Add-AzureCacheWorkerRole CacheRole
Enable-AzureMemcacheRole WebRole1 CacheRole

md "temp"
Copy-Item "$scriptFolder\Test\MemCacheClientNodeJS.exe" ".\temp\"
Start-Process '.\temp\MemCacheClientNodeJS.exe' "-y" -Wait
Copy-Item ".\Temp\*" ".\WebRole1\"  -Force -Exclude "MemCacheClientNodeJS.exe" -Recurse

cd "$testFolder\MemCacheTestWithNode"
Start-AzureEmulator -v

Write-Host "You can do some testing by loading role url in the browser and adding some key/value to mem cache emulators" -ForegroundColor "Yellow"
Write-Host "Press any key to continue to the next testing"
$keyPressed = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")

Write-Host "Testing PHP web & worker roles with emulator" -ForegroundColor "Green" 
cd $testFolder
New-AzureServiceProject MyPHPTest
Add-AzurePHPWebRole
Add-AzurePHPWorkerRole
Start-AzureEmulator -v

Write-Host "You can do some testing by loading role url in the browser and make sure PHP default pages loads" -ForegroundColor "Yellow"
Write-Host "Press any key to continue to the next testing"
$keyPressed = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")

Write-Host "Testing Django web roles" -ForegroundColor "Green"
cd $testFolder
New-AzureServiceProject MyDjangoTest
Add-AzureDjangoWebRole
Start-AzureEmulator  -v 
Write-Host "You can do some testing by loading role url in the browser and make sure default django page loads fine " -ForegroundColor "Yellow"