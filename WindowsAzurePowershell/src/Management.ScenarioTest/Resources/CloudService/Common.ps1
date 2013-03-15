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

$createdCloudServices = @()

<#
.SYNOPSIS
Gets valid and available cloud service name.
#>
function Get-CloudServiceName
{
	do
	{
		$name = "onesdk" + (Get-Random).ToString()
		$used = Test-AzureName -Service $name
	} while ($used)

	return $name
}

<#
.SYNOPSIS
Gets the default location
#>
function Get-DefaultLocation
{
	return (Get-AzureLocation)[0].Name
}

<#
.SYNOPSIS
Creates cloud services with the count specified

.PARAMETER count
The number of cloud services to create.
#>
function New-CloudService
{
	param([int] $count, [ScriptBlock] $cloudServiceProject, [string] $slot)
	
	if ($cloudServiceProject -eq $null) { $cloudServiceProject = { New-TinyCloudServiceProject $args[0] } }
	if ($slot -eq $null) { $slot = "Production" }

	1..$count | % { 
		$name = Get-CloudServiceName;
		Invoke-Command -ScriptBlock $cloudServiceProject -ArgumentList $name;
		Publish-AzureServiceProject -Force -Slot $slot
		$global:createdCloudServices += $name;
	}
}

<#
.SYNOPSIS
Removes all cloud services/storage accounts in the current subscription.
#>
function Initialize-CloudServiceTest
{
	Get-AzureStorageAccount | Remove-AzureStorageAccount
	Get-AzureService | Remove-AzureService -Force
}

<#
.SYNOPSIS
Creates new cloud service project with one node web role.
#>
function New-TinyCloudServiceProject
{
	param([string] $name)

	New-AzureServiceProject $name
	Add-AzureNodeWebRole
}