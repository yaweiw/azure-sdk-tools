@echo off

set output=..\..\Package\Release
set serviceManagementResources=%output%\AzureServiceManagement\Resources

echo Removing Resources folder %serviceManagementResources%
rmdir /S /Q %serviceManagementResources%

echo Removing generated NuGet files from %output%
rmdir /S /Q %output%\AzureServiceManagement\de
rmdir /S /Q %output%\AzureServiceManagement\es
rmdir /S /Q %output%\AzureServiceManagement\fr
rmdir /S /Q %output%\AzureServiceManagement\it
rmdir /S /Q %output%\AzureServiceManagement\ja
rmdir /S /Q %output%\AzureServiceManagement\ko
rmdir /S /Q %output%\AzureServiceManagement\ru
rmdir /S /Q %output%\AzureServiceManagement\zh-Hans
rmdir /S /Q %output%\AzureServiceManagement\zh-Hant
rmdir /S /Q %output%\AzureResourceManager\de
rmdir /S /Q %output%\AzureResourceManager\es
rmdir /S /Q %output%\AzureResourceManager\fr
rmdir /S /Q %output%\AzureResourceManager\it
rmdir /S /Q %output%\AzureResourceManager\ja
rmdir /S /Q %output%\AzureResourceManager\ko
rmdir /S /Q %output%\AzureResourceManager\ru
rmdir /S /Q %output%\AzureResourceManager\zh-Hans
rmdir /S /Q %output%\AzureResourceManager\zh-Hant

echo Delete XML help files for helper dlls from %output%
:: The xml help files are not being deleted.

echo Delete config files for dlls from %output%
del %output%\AzureServiceManagement\*.dll.config
del %output%\AzureResourceManager\*.dll.config

heat dir %output% -srd -gg -g1 -cg azurecmdfiles -sfrag -dr PowerShellFolder -var var.sourceDir -o azurecmdfiles.wxi