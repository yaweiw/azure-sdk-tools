
$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupEnv.ps1')

#Get WebPI CMD
$WebPi="$scriptFolder\test\WebpiCmd.exe"

$allWebPIVersions = Get-ChildItem HKLM:\SOFTWARE\Microsoft\WebPlatformInstaller -ea SilentlyContinue | 
    ForEach-Object {  
        if($_.GetValue("InstallPath", $null) -ne $null)  
        {
            $WebPi = $_.GetValue("InstallPath")  + "WebpiCmd.exe"
        }
    }

Write-Host "using webpi command: $WebPi"

$programFiles = $env:ProgramFiles
if (Test-Path "$env:ProgramW6432"){
    $programFiles = $env:ProgramW6432
}

if (!(Test-Path "$programFiles\Microsoft SDKs\Windows Azure\.NET SDK\v2.3")) {
    Write-Host installing Azure Authoring Tools
    Start-Process "$WebPi" "/Install /products:WindowsAzureSDK_Only_2_3 /accepteula" -Wait
}

if (!(Test-Path "$programFiles\Microsoft SDKs\Windows Azure\Emulator")) {
    Write-Host installing Azure Compute Emulator
    Start-Process "$WebPi" "/Install /products:WindowsAzureEmulator_Only_2_3 /accepteula" -Wait
}

if (!(Test-Path "$env:ADXSDKProgramFiles\Microsoft SDKs\Windows Azure\Storage Emulator")) {
    Write-Host installing Azure Storage Emulator
    Start-Process "$WebPi" "/Install /products:WindowsAzureStorageEmulator /accepteula" -Wait
}

try {
  git.exe| Out-Null
}
catch [System.Management.Automation.CommandNotFoundException] {
    if (Test-Path "$env:ADXSDKProgramFiles\Git\bin") {
        Write-Host Adding Git installation folder to the PATH environment variable, needed for 2 unit tests
        $env:path = $env:path + ";$env:ADXSDKProgramFiles\Git\bin"
    }
}

#The detecting logic for django is not decent, but the best we can do so far.    
if (!(Test-Path "$env:SystemDrive\Python27")) {
    Write-Host "download Python, Pip and Django to $tempFileShare"
    $tempFileShare = $env:temp
    $client = New-Object System.Net.WebClient
    $client.DownloadFile("https://www.python.org/ftp/python/2.7.5/python-2.7.5.msi", "$tempFileShare\python-2.7.5.msi")
    $client.DownloadFile("https://raw.github.com/pypa/pip/master/contrib/get-pip.py", "$tempFileShare\get-pip.py");        
    Write-Host "Install..."
    Start-Process msiexec.exe "/i $tempFileShare\python-2.7.5.msi /passive" -Wait
    Start-Process "$env:SystemDrive\Python27\python.exe" "$tempFileShare\get-pip.py" -Wait
    Start-Process "$env:SystemDrive\Python27\scripts\pip.exe" "install Django==1.5" -Wait
    Remove-Item "$tempFileShare\python-2.7.5.msi"
    Remove-Item "$tempFileShare\get-pip.py"
}

$env:AZURE_TEST_MODE="Playback"
msbuild.exe $env:AzurePSRoot\..\build.proj /t:test
