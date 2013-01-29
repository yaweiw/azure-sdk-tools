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
		$name = "OneSDK" + (Get-Random).ToString()
		$available = Test-AzureName -Service $name
	} while (!$available)

	return $name
}

<#
.SYNOPSIS
Creates cloud services with the count specified

.PARAMETER count
The number of cloud services to create.
#>
function New-CloudService
{
	param([int] $count, [ScriptBlock] $cloudServiceProject)
	
	if ($cloudServiceProject -eq $null) { $cloudServiceProject = { New-TinyCloudServiceProject $args[0] } }

	1..$count | % { 
		$name = Get-CloudServiceName;
		Invoke-Command -ScriptBlock $cloudServiceProject -ArgumentList $name;
		Publish-AzureServiceProject -Force;
		$global:createdCloudServices += $name;
	}
}

<#
.SYNOPSIS
Creates cloud services and runs validation the count specified

.PARAMETER count
The number of cloud services to create.
#>
function Verify-CloudService
{
	param([int] $count, [ScriptBlock] $cloudServiceProject, [ScriptBlock] $verifier)
	$success = $true
	if ($cloudServiceProject -eq $null) { $cloudServiceProject = { New-TinyCloudServiceProject $args[0] } }
	if ($verifier -eq $null) {$verifier = {return $true}}
	1..$count | % { 
		$name = Get-CloudServiceName;
		Invoke-Command -ScriptBlock $cloudServiceProject -ArgumentList $name;
		$service = Publish-AzureServiceProject -Force;
		$global:createdCloudServices += $name;
		Invoke-Command -ScriptBlock $verifier -ArgumentList $service -OutVariable $worked
		$success = $success -and $worked
	}

	return $success
}

<#
.SYNOPSIS
Removes all cloud services/storage accounts in the current subscription.
#>
function Initialize-CloudServiceTest
{
	<# To Do: implement when we have unsigned version from Management.ServiceManagement assembly #>
	$global:createdCloudServices = @()
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

<#
.SYNOPSIS
Creates new cloud service project with a web role connected to a cache.
#>
function New-CacheCloudServiceProject
{
	param([string] $name)

	New-AzureServiceProject $name
	Add-AzureNodeWebRole ClientRole
	copy ..\CloudService\Cache\*.js .\ClientRole\
	cd .\ClientRole
	npm install ..\..\CloudService\Cache\mc.tgz ..\..\CloudService\Cache\connman.tgz
	cd ..
	Add-AzureCacheWorkerRole CacheRole
	Enable-AzureMemcacheRole ClientRole CacheRole
}

<#
.SYNOPSIS
Places and retrieves a key value pair from a cache app
#>
function Verify-CacheApp
{
    param([string]$uri)
	$client = New-Object System.Net.WebClient
	$client.BaseAddress = $uri
	$toss = $client.UploadString("/add", "key=key1&value=value1")
	$check = $client.UploadString("/get", "key=key1")
	return $check.Contains("key1") -and $check.Contains("value1")
}