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

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with empty add-ons.
#>
function Test-TestGetAzureStoreAddOnWithNoAddOns
{
	# Setup
	$current = Get-AzureStoreAddOn

	if ($current.Count -eq 0)
	{
		Write-Warning "The test can't run because the account is not setup correctly (add-on count should be 0)";
		exit;
	}

	# Test
	$actual = Get-AzureStoreAddOn

	# Assert
	Assert-True { $actual.Count -eq 0 }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with one add-on.
#>
function Test-TestGetAzureStoreAddOnWithOneAddOn
{
	# Setup
	$current = Get-AzureStoreAddOn

	if ($current.Count -eq 0)
	{
		Write-Warning "The test can't run because the account is not setup correctly (add-on count should be 0)";
		exit;
	}
	New-AddOn

	# Test
	$actual = Get-AzureStoreAddOn

	# Assert
	Assert-True { $actual.Count -eq 1 }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with many add-ons
#>
function Test-GetAzureStoreAddOnWithMultipleAddOns
{
	# Setup
	New-AddOn 3

	# Test
	$actual = Get-AzureStoreAddOn

	# Assert
	Assert-True { $actual.Count -gt 1 }
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with getting existing add-on
#>
function Test-GetAzureStoreAddOnWithExistingAddOn
{
	# Setup
	New-AddOn
	$expected = $global:createdAddOns[0]

	# Test
	$actual = Get-AzureStoreAddOn $global:createdAddOns[0]

	# Assert
	$actual = $actual[0].Name
	Assert-AreEqual $expected $actual
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with case invesitive.
#>
function Test-GetAzureStoreAddOnCaseInsinsitive
{
	# Setup
	New-AddOn
	$expected = $global:createdAddOns[0]

	# Test
	$actual = Get-AzureStoreAddOn $expected.ToUpper()

	# Assert
	$actual = $actual[0].Name
	Assert-AreEqual $expected.ToUpper() $actual.ToUpper()
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with invalid add-on name, expects to fail.
#>
function Test-GetAzureStoreAddOnWithInvalidName
{
	# Test
	Assert-Throws { Get-AzureStoreAddOn "Invalid Name" } "The provided add-on name 'Invalid Name' is invalid"
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with valid and non-existing add-on.
#>
function Test-GetAzureStoreAddOnValidNonExisting
{
	# Test
	$actual = Get-AzureStoreAddOn "NonExistingAddOn"

	# Assert
	Assert-AreEqual 0 $actual.Count
}

<#
.SYNOPSIS
Tests using Get-AzureStoreAddOn with App Service
#>
function Test-GetAzureStoreAddOnWithAppService
{
	# Setup
	New-AddOn

	# Test
	$actual = Get-AzureStoreAddOn

	# Assert
	$addon = $actual[0]
	Assert-AreEqual "App Service" $addon.Type
}

<#
.SYNOPSIS
Tests the piping between Get-AzureAddOn and Remove-AzureAddOn
#>
function Test-GetAzureStoreAddOnPipedToRemoveAzureAddOn
{
	# Setup
	New-AddOn
	$name = $global:createdAddOns[0]

	# Test
	Get-AzureStoreAddOn $name | Remove-AzureStoreAddOn

	# Assert
	$actual = Get-AzureStoreAddOn $name
	Assert-AreEqual 0 $actual.Count
}