@echo off

set outdir=%1
set armDest=%outdir%..\..\ResourceManager\AzureResourceManager

xcopy /y %outdir%ServiceManagementStartup.ps1 %armDest%