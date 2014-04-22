$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupEnv.ps1')

msbuild "$env:AzurePSRoot\..\build.proj" /t:"BuildDebug;buildsetupdebug"
Write-Host "MSI file path: $env:AzurePSRoot\setup\build\Debug\x86\windowsazure-powershell.msi"