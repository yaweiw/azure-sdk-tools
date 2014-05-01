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

$createdProfiles = @()
$currentProfile = $null

<#
.SYNOPSIS
Gets valid profile name.
#>
function Get-ProfileName
{
	return [guid]::NewGuid() + "profile"
}

<#
.SYNOPSIS
Creates a profile.
#>
function New-Profile
{
	param([string] $profileName)

    New-AzureTrafficManagerProfile -DomainName ${profileName}.trafficmanager.net -LoadBalancingMethod "RoundRobin" -MonitorPort 80 -MonitorProtocol "http" -MonitorRelativePath "/" -Name ${Get-ProfileName} -Ttl 300
}

<#
.SYNOPSIS
Removes all profiles in the current subscription.
#>
function Initialize-WebsiteTest
{
	Get-AzureTrafficManagerProfile  | Remove-AzureTrafficManagerProfile -Force
}
