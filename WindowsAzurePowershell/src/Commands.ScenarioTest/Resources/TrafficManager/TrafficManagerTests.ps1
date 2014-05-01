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

########################################################################### General TrafficManager Scenario Tests######################################################################

<#
.SYNOPSIS
Tests any cloud based cmdlet with invalid credentials and expect it'll throw an exception.
#>
function Test-WithInvalidCredentials
{
	param([ScriptBlock] $cloudCmdlet)
	
	# Setup
	Remove-AllSubscriptions

	# Test
	Assert-Throws $cloudCmdlet "No current subscription has been designated. Use Select-AzureSubscription -Current &lt;subscriptionName&gt; to set the current subscription."
}

########################################################################### Remove-Profile Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Remove-Profile with existing name
#>
function Test-RemoveProfileWithValidName
{
	$profileName = ${Get-ProfileName}

	# Setup
	New-Profile $profileName
	$expected = "The profile $name was not found. Please specify a valid profile name."

	# Test
	Remove-AzureTrafficManagerProfile -Name $profileName -Force

	# Assert
	Assert-Throws { Get-AzureTrafficManagerProfile -Name $profileName } $expected
}

<#
.SYNOPSIS
Tests Remove-Profile with non existing name
#>
function Test-RemoveProfileWithNonExistingName
{
	Assert-Throws { Remove-AzureTrafficManagerProfile -Name "nonexistingprofile" -Force } "The profile nonexistingprofile was not found. Please specify a valid profile name."
}

########################################################################### Get-Profile Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Get-Profile
#>
function Test-GetProfile
{
	$profileName = ${Get-ProfileName}
	
	$createdProfile = New-Profile $profileName
	
	$retrievedProfile = Get-AzureTrafficManagerProfile -name $profileName
	
	Assert-AreEqualObject $createdProfile $retrievedProfile
}

<#
.SYNOPSIS
Tests Get-Profiles
#>
function Test-GetProfiles
{
	$profileName1 = ${Get-ProfileName}
	$profileName2 = ${Get-ProfileName}

	$createdProfile1 = New-Profile $profileName1
	$createdProfile2 = New-Profile $profileName2
	
	$retrievedProfiles = Get-AzureTrafficManagerProfile
	
	$expectedProfiles = $profileName1, $profileName2
	
	Assert-AreEqualObject  $expectedProfiles, $retrievedProfiles 
}

<#
.SYNOPSIS
Tests Get-Profile for a non existing profile
#>
function Test-GetProfileNonExistingName
{
	Assert-Throws { Get-AzureTrafficManagerProfile -name "nonexistingprofile" -Force } "The profile nonexistingprofile was not found. Please specify a valid profile name."
}


###########################################################################  ###########################################################################
