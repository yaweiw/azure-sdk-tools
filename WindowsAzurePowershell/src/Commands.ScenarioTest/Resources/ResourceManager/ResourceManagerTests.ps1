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
	
	$list = Get-AzureResource -ResourceGroupName $rgname

	# Assert
	Assert-AreEqual $expected.Name $actual.Name
	Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName
	Assert-AreEqual $expected.ResourceType $actual.ResourceType
	Assert-AreEqual 1 $list.Count
	Assert-AreEqual $expected.Name $list[0].Name
	
	# Cleanup
	Remove-AzureResourceGroup -Name $rgname -Force
}

function Test-CreatesNewComplexResource
{
	# Setup
	$rgname = Get-ResourceGroupName
	$rnameParent = Get-ResourceName
	$rnameChild = Get-ResourceName
	$location = Get-ResourceDefaultLocation

	# Test
	New-AzureResourceGroup -Name $rgname -Location $location
	$actualParent = New-AzureResource -Name $rnameParent -Location "West US" -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers" -PropertyObject @{"administratorLogin" = "adminuser"; "administratorLoginPassword" = "P@ssword1"} -ApiVersion 2004-04-01
	$expectedParent = Get-AzureResource -Name $rnameParent -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers" -ApiVersion 2004-04-01

	$actualChild = New-AzureResource -Name $rnameChild -Location "West US" -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/databases" -ParentResource servers/$rnameParent -PropertyObject @{"edition" = "Web"; "collation" = "SQL_Latin1_General_CP1_CI_AS"; "maxSizeBytes" = "1073741824"} -ApiVersion 2004-04-01
	$expectedChild = Get-AzureResource -Name $rnameChild -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/databases" -ParentResource servers/$rnameParent -ApiVersion 2004-04-01

	$list = Get-AzureResource -ResourceGroupName $rgname

	$parentFromList = $list | where {$_.Name.Contains("/") -eq $false} | Select-Object -First 1
	$childFromList = $list | where {$_.Name.Contains("/")} | Select-Object -First 1

	# Assert
	Assert-AreEqual $expectedParent.Name $actualParent.Name
	Assert-AreEqual $expectedChild.Name $actualChild.Name
	Assert-AreEqual $expectedParent.ResourceType $actualParent.ResourceType
	Assert-AreEqual $expectedChild.ResourceType $actualChild.ResourceType

	Assert-AreEqual 2 $list.Count
	Assert-AreEqual $expectedParent.Name $parentFromList.Name
	Assert-AreEqual "$($expectedParent.Name)/$($expectedChild.Name)" $childFromList.Name
	Assert-AreEqual $expectedParent.ResourceType $parentFromList.ResourceType
	Assert-AreEqual $expectedChild.ResourceType $childFromList.ResourceType
	
	# Cleanup
	Remove-AzureResourceGroup -Name $rgname -Force
}