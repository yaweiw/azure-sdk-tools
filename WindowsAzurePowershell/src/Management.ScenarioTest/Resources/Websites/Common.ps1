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

$createdWebsites = @()

<#
.SYNOPSIS
Gets valid website name.
#>
function Get-WebsiteName
{
	return "OneSDKWebsite" + (Get-Random).ToString()
}

<#
.SYNOPSIS
Creates websites with the count specified

.PARAMETER count
The number of websites to create.
#>
function New-Website
{
	param([int] $count)
	
	1..$count | % {
		$name = Get-WebsiteName
		New-AzureWebsite $name
		$global:createdWebsites += $name
	}
}

<#
.SYNOPSIS
Removes all websites in the current subscription.
#>
function Initialize-WebsiteTest
{
	Get-AzureWebsite | Remove-AzureWebsite -Force
}

<#
.SYNOPSIS
Clones git repo
#>
function Clone-GitRepo
{
	param([string] $repo, [string] $dir)

	$cloned = $false
	do
	{
		try
		{
			git clone $repo $dir
			$cloned = $true
		}
		catch
		{
			# Do nothing
		}
	}
	while (!$cloned)
}