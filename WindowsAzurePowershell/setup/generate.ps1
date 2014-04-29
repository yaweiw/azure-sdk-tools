param(
    [Parameter(Mandatory = $false, Position = 0)]
    [string] $buildConfig
)

if ($buildConfig -eq $null -or ($args[0] -ne "Debug" -and $args -ne "Release"))
{
	$buildConfig = 'Release'
}

Write-Verbose "Build configuration is set to $buildConfig"
$output = Join-Path $(Split-Path -Parent -Path (Split-Path -Parent -Path $PSScriptRoot)) $(Join-Path 'Package' $buildConfig)
$serviceManagementPath = Join-Path $output "ServiceManagement\Azure"
$resourceManagerPath = Join-Path $output "ResourceManager\AzureResourceManager"
$VerbosePreference = 'Continue'

Write-Verbose "Removing duplicated AzureProfile.psd1..."
Remove-Item -Force $serviceManagementPath\AzureProfile.psd1

Write-Verbose "Removing Resources folder $serviceManagementPath\Resources\..."
Remove-Item -Recurse -Force $serviceManagementPath\Resources\

Write-Verbose "Removing generated NuGet files from $output"
$resourcesFolders = @("de", "es", "fr", "it", "ja", "ko", "ru", "zh-Hans", "zh-Hant")
Get-ChildItem -Include $resourcesFolders -Recurse -Force -Path $output | Remove-Item -Force -Recurse

Write-Verbose "Removing XML help files for helper dlls from $output"
$execlusions = @("*.dll-Help.xml", "Scaffold.xml", "RoleSettings.xml", "WebRole.xml", "WorkerRole.xml")
Get-ChildItem -Include *.xml -Exclude $execlusions -Recurse -Path $output | Remove-Item -Force -Recurse

Write-Verbose "Removing config files for dlls from $output"
Remove-Item -Recurse -Force $output\*.dll.config

if (Get-Command "heat.exe" -ErrorAction SilentlyContinue)
{
    $azureFiles = $(Join-Path $PSScriptRoot 'azurecmdfiles.wxi')
    heat dir $output -srd -gg -g1 -cg azurecmdfiles -sfrag -dr PowerShellFolder -var var.sourceDir -o $(Join-Path $PSScriptRoot 'azurecmdfiles.wxi')
    
	# Replace <Wix> with <Include>
	(gc $azureFiles).replace('<Wix','<Include') | Set-Content $azureFiles
	(gc $azureFiles).replace('</Wix','</Include') | Set-Content $azureFiles
}
else
{
    Write-Error "Failed to execute heat.exe, the Wix bin folder is not in PATH"
}