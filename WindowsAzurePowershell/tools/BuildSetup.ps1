$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupEnv.ps1')

msbuild "$env:AzurePSRoot\..\build.proj" /t:buildsetupdebug
Write-Host MSI file path: %AzurePSRoot%\setup\build\Debug\x86