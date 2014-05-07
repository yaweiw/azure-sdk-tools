# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupEnv.ps1')

$packageFolder="$env:AzurePSRoot\..\Package"
if (Test-Path $packageFolder) {
    Remove-Item -Path "$env:AzurePSRoot\..\Package" -Force -Recurse	
}

$wixInstalled = $false
$keyPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
if (Test-Path ${env:\ProgramFiles(x86)} ){
    $keyPath = "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
}

$allProducts = Get-ChildItem $keyPath

foreach ($product in $allProducts){
    $displayName = $product.GetValue("DisplayName", $null)
    if (($displayName -ne $null) -and ($displayName.StartsWith("Windows Installer XML Toolset") -or $displayName.StartsWith("WiX Toolset"))) {
        $wixInstalled = $true
        Write-Verbose "WIX tools was installed"
        break
    }
}

if (!($wixInstalled)){
     Write-Host "You don't have Windows Installer XML Toolset installed, which is needed to build setup." -ForegroundColor "Yellow"
     Write-Host "Press (Y) to install through codeplex web page we will open for you; (N) to skip"    
     $keyPressed = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp")
     if ($keyPressed.Character -eq "y" ){
        Invoke-Expression "cmd.exe /C start http://wix.codeplex.com/downloads/get/762937"
        Read-Host "Press any key to continue after the installtion is finished"
     }
}

#add wix to the PATH. Note, no need to be very accurate here, 
#and we just register both 3.8 & 3.5 to simplify the script
$env:path = $env:path + ";$env:ADXSDKProgramFiles\WiX Toolset v3.8\bin;$env:ADXSDKProgramFiles\Windows Installer XML v3.5\bin"

# Build the cmdlets in debug mode
msbuild "$env:AzurePSRoot\..\build.proj" /t:"BuildDebug"

# Regenerate the installer files
&"$env:AzurePSRoot\setup\generate.ps1" 'Debug'

# Build the installer
msbuild "$env:AzurePSRoot\..\build.proj" /t:"BuildSetupDebug"

Write-Host "MSI file path: $env:AzurePSRoot\setup\build\Debug\x86\windowsazure-powershell.msi"