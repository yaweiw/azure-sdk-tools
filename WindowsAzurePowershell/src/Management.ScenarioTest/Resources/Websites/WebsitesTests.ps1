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

########################################################################### General Websites Scenario Tests ###########################################################################

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
	Assert-Throws $cloudCmdlet "Call Set-AzureSubscription and Select-AzureSubscription first."
}

########################################################################### Remove-AzureWebsite Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Remove-AzureWebsite with existing name
#>
function Test-RemoveAzureServiceWithValidName
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with non existing name
#>
function Test-RemoveAzureServiceWithNonExistingName
{
	Assert-Throws { Remove-AzureWebsite "OneSDKNotExisting" -Force } "The website OneSDKNotExisting was not found. Please specify a valid website name."
}

<#
.SYNOPSIS
Tests Remove-AzureWebsite with WhatIf
#>
function Test-RemoveAzureServiceWithWhatIf
{
	# Setup
	$name = Get-WebsiteName
	New-AzureWebsite $name
	$expected = "The website $name was not found. Please specify a valid website name."

	# Test
	Remove-AzureWebsite $name -Force -WhatIf
	Remove-AzureWebsite $name -Force

	# Assert
	Assert-Throws { Get-AzureWebsite $name } $expected
}