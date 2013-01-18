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

<#
.SYNOPSIS
Tests using List-AzureSBLocation and make sure that it's contents are filled out correctly.
#>
function Test-ListAzureSBLocation
{
	Get-AzureSBLocation | % { Assert-NotNull $_.Code;Assert-NotNull $_.FullName }
}

<#
.SYNOPSIS
Tests using List-AzureSBLocation and piping it's output to New-AzureSBNamespace.
#>
function Test-ListAzureSBLocation1
{
	# Setup
	$expectedName = Get-NamespaceName
	$expectedLocation = Get-DefaultLocation

	# Test
	$namespace = Get-AzureSBLocation | 
	Select @{Name="Location";Expression={$_."Code"}} | 
	Where {$_.Location -eq $expectedLocation} | 
	% { New-Object PSObject -Property @{Name=$expectedName;Location=$_.Location} } | 
	New-AzureSBNamespace
  
	# Assert
	$actualName = $namespace.Name
	$actualLocation = $namespace.Region
	Assert-AreEqual $expectedName $actualName
	Assert-AreEqual $expectedLocation  $actualLocation

	# Cleanup
	$createdNamespaces += $expectedName
	Test-CleanupServiceBus
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet and expects that no namespaces are returned.
#>
function Test-GetAzureSBNamespaceWithEmptyNamespaces
{
	# Setup
	Initialize-NamespaceTest

	# Test
	$namespaces = Get-AzureSBNamespace

	# Assert
	Assert-AreEqual $namespaces.Count 0
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet and expects that one namespace is returned.
#>
function Test-GetAzureSBNamespaceWithOneNamespace
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1

	# Test
	$namespaces = Get-AzureSBNamespace

	# Assert
	Assert-AreEqual $namespaces.Count 1

	# Cleanup
	Test-CleanupServiceBus
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet and expects that multiple namespaces are returned.
#>
function Test-GetAzureSBNamespaceWithMultipleNamespaces
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 3

	# Test
	$namespaces = Get-AzureSBNamespace

	# Assert
	Assert-AreEqual $namespaces.Count 3

	# Cleanup
	Test-CleanupServiceBus
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet using a valid name and expects getting the same object back.
#>
function Test-GetAzureSBNamespaceWithValidExisitingNamespace
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$expectedName = $createdNamespaces[0]

	# Test
	$namespace = Get-AzureSBNamespace $expectedName

	# Assert
	Assert-NotNull $namespace
	$actualName = $namespace.Name
	Assert-AreEqual $expectedName $actualName

	# Cleanup
	Test-CleanupServiceBus
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet using a non-existing name and expects that an exception is thrown.
#>
function Test-GetAzureSBNamespaceWithValidNonExisitingNamespace
{
	# Setup
	$invalidName = "OneSDKNotCreated"

	# Test
	Assert-Throws { Get-AzureSBNamespace $invalidName } "Internal Server Error. This could happen due to an incorrect/missing namespace"
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet and pipe it's result to Remove-AzureSBNamespace cmdlet.
#>
function Test-GetAzureSBNamespacePipedToRemoveAzureSBNamespace
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 5
	$actual = $true

	# Test
	Get-AzureSBNamespace | Remove-AzureSBNamespace -PassThru | % {$actual = $actual -and $_}

	# Assert
	Assert-True { $actual } "Piping Get-AzureSBNamespace into Remove-AzureSBNamespace failed"

	# Cleanup
	Test-CleanupServiceBus
}

<#
.SYNOPSIS
Tests running Get-AzureSBNamespace cmdlet and pipe it's result to Set-AzureWebsite cmdlet.
#>
function Test-GetAzureSBNamespaceWithWebsites
{
	# Setup
	# Initialize-NamespaceTest
	$namespaceName = Get-NamespaceName
	New-AzureSBNamespace $namespaceName $(Get-DefaultLocation)
	Wait-NamespaceStatus $namespaceName "Active"
	$websiteName = Get-NamespaceName
	New-AzureWebsite $websiteName
	$settingName = "NamespaceConnectionString"

	# Test
	Get-AzureSBNamespace $namespaceName | % { Set-AzureWebsite $websiteName -AppSettings @{ $settingName = $_.ConnectionString } }

	# Assert
	$website = Get-AzureWebsite $websiteName
	$namespace = Get-AzureSBNamespace $namespaceName
	Assert-AreEqual $namespace.ConnectionString $website.AppSettings[$settingName]

	# Cleanup
	$createdNamespaces += $namespaceName
	Remove-AzureWebsite $websiteName
	Test-CleanupServiceBus
}