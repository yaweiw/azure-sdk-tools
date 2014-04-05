@echo off

set outdir=%1

:: This block copies AzureProfile.psd1 in a separate folder so it is a "well-formed" module. For more information: http://msdn.microsoft.com/en-us/library/dd878350%28v=vs.85%29.aspx
md %outdir%AzureProfile
xcopy /y %outdir%AzureProfile.psd1 %outdir%AzureProfile
del %outdir%AzureProfile.psd1
xcopy /y %outdir%Microsoft.WindowsAzure.Commands.dll-help.xml %outdir%AzureProfile
