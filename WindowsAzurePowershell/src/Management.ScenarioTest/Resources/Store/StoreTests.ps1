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

########################################################################### General Store Scenario Tests ###########################################################################

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

########################################################################### Get-AzureStoreAddOn -ListAvailable Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn -ListAvailable with default country (US)
#>
function Test-GetAzureStoreAddOnListAvailableWithDefaultCountry
{
	# Test
	$actual = Get-AzureStoreAddOn -ListAvailable

	# Assert
	Assert-True { $actual.Count -gt 0 }
	$actual | % { Assert-NotNull $_.Provider; Assert-NotNull $_.AddOn; Assert-NotNull $_.Plans Assert-NotNull $_.Locations }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn -ListAvailable with specified country that will not return any addons.
#>
function Test-GetAzureStoreAddOnListAvailableWithNoAddOns
{
	# Test
	$actual = Get-AzureStoreAddOn -ListAvailable "E1"

	# Assert
	Assert-True { $actual.Count -eq 0 }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn -ListAvailable with specified country that will return addons.
#>
function Test-GetAzureStoreAddOnListAvailableWithCountry
{
	# Test
	$actual = Get-AzureStoreAddOn -ListAvailable "CH"

	# Assert
	Assert-True { $actual.Count -gt 0 }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn -ListAvailable with invalid country name.
#>
function Test-GetAzureStoreAddOnListAvailableWithInvalidCountryName
{
	# Test
	Assert-Throws { Get-AzureStoreAddOn -ListAvailable "UnitedStates" } "Cannot validate argument on parameter 'Country'. The country name is invalid, please use a valid two character country code, as described in ISO 3166-1 alpha-2."
}