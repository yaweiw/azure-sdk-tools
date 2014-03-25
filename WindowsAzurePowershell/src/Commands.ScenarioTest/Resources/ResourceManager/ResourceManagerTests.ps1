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
Tests creating new simple resource group.
#>
function Test-CreatesNewSimpleResourceGroup
{
	# Setup
	$name = Get-ResourceGroupName
	$location = Get-ResourceGroupDefaultLocation

	# Test
	$actual = New-AzureResourceGroup -Name $name -Location $location
	$expected = Get-AzureResourceGroup -Name $name

	# Assert
	Assert-AreEqual $expected.Name $actual.Name
	
	# Cleanup
	Remove-AzureResourceGroup -Name $name -Force
}

function Test-CreatesNewSimpleResource
{
	# Setup
	$rgname = Get-ResourceGroupName
	$rname = Get-ResourceName
	$location = Get-ResourceDefaultLocation

	# Test
	New-AzureResourceGroup -Name $rgname -Location $location
	$actual = New-AzureResource -Name $rname -Location $location -ResourceGroupName $rgname -ResourceType "Microsoft.Web/sites" -PropertyObject @{"name" = $name; "siteMode" = "Limited"; "computeMode" = "Shared"} -ApiVersion 2004-04-01
	$expected = Get-AzureResource -Name $rname -ResourceGroupName $rgname -ResourceType "Microsoft.Web/sites" -ApiVersion 2004-04-01

	# Assert
	Assert-AreEqual $expected.Name $actual.Name
	Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName
	Assert-AreEqual $expected.ResourceType $actual.ResourceType
	
	# Cleanup
	Remove-AzureResourceGroup -Name $rgname -Force
}