@echo off

set output=..\..\Package\Release
set serviceManagementResources=%output%\Azure\Resources

echo Removing Resources folder %serviceManagementResources%
rmdir /S /Q %serviceManagementResources%

echo Removing generated NuGet files from %output%
rmdir /S /Q %output%\Azure\de
rmdir /S /Q %output%\Azure\es
rmdir /S /Q %output%\Azure\fr
rmdir /S /Q %output%\Azure\it
rmdir /S /Q %output%\Azure\ja
rmdir /S /Q %output%\Azure\ko
rmdir /S /Q %output%\Azure\ru
rmdir /S /Q %output%\Azure\zh-Hans
rmdir /S /Q %output%\Azure\zh-Hant
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
del %output%\Azure\*.dll.config
del %output%\AzureResourceManager\*.dll.config

heat dir %output% -srd -gg -g1 -cg azurecmdfiles -sfrag -dr PowerShellFolder -var var.sourceDir -o azurecmdfiles.wxi