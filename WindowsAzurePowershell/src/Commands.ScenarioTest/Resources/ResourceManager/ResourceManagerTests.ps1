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
	$rgname = Get-ResourceGroupName
	$location = Get-ProviderLocation ResourceManagement

	Try 
	{
		# Test
		$actual = New-AzureResourceGroup -Name $rgname -Location $location
		$expected = Get-AzureResourceGroup -Name $rgname

		# Assert
		Assert-AreEqual $expected.Name $actual.Name	
	}
	Finally
	{
		# Cleanup
		Remove-AzureResourceGroup -Name $rgname -Force
	}
}

function Test-CreatesNewSimpleResource
{
	# Setup
	$rgname = Get-ResourceGroupName
	$rname = Get-ResourceName
	$rglocation = Get-ProviderLocation ResourceManagement
	$location = Get-ProviderLocation "Microsoft.Web/sites"
	$apiversion = "2014-04-01"

	# Test
	Try 
	{
		New-AzureResourceGroup -Name $rgname -Location $rglocation
		$actual = New-AzureResource -Name $rname -Location $location -ResourceGroupName $rgname -ResourceType "Microsoft.Web/sites" -PropertyObject @{"name" = $name; "siteMode" = "Limited"; "computeMode" = "Shared"} -ApiVersion $apiversion
		$expected = Get-AzureResource -Name $rname -ResourceGroupName $rgname -ResourceType "Microsoft.Web/sites" -ApiVersion $apiversion
	
		$list = Get-AzureResource -ResourceGroupName $rgname

		# Assert
		Assert-AreEqual $expected.Name $actual.Name
		Assert-AreEqual $expected.ResourceGroupName $actual.ResourceGroupName
		Assert-AreEqual $expected.ResourceType $actual.ResourceType
		Assert-AreEqual 1 $list.Count
		Assert-AreEqual $expected.Name $list[0].Name	
	}
	Finally
	{
		# Cleanup
		Remove-AzureResourceGroup -Name $rgname -Force
	}
}

function Test-CreatesNewComplexResource
{
	# Setup
	$rgname = Get-ResourceGroupName
	$rnameParent = Get-ResourceName
	$rnameChild = Get-ResourceName
	$rglocation = Get-ProviderLocation ResourceManagement
	$location = Get-ProviderLocation "Microsoft.Sql/servers"
	$apiversion = "2014-04-01"

	# Test
	Try 
	{
		New-AzureResourceGroup -Name $rgname -Location $rglocation
		$actualParent = New-AzureResource -Name $rnameParent -Location $location -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers" -PropertyObject @{"administratorLogin" = "adminuser"; "administratorLoginPassword" = "P@ssword1"} -ApiVersion $apiversion
		$expectedParent = Get-AzureResource -Name $rnameParent -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers" -ApiVersion $apiversion

		$actualChild = New-AzureResource -Name $rnameChild -Location $location -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers/databases" -ParentResource servers/$rnameParent -PropertyObject @{"edition" = "Web"; "collation" = "SQL_Latin1_General_CP1_CI_AS"; "maxSizeBytes" = "1073741824"} -ApiVersion $apiversion
		$expectedChild = Get-AzureResource -Name $rnameChild -ResourceGroupName $rgname -ResourceType "Microsoft.Sql/servers/databases" -ParentResource servers/$rnameParent -ApiVersion $apiversion

		$list = Get-AzureResource -ResourceGroupName $rgname

		$parentFromList = $list | where {$_.ResourceType -eq 'Microsoft.Sql/servers'} | Select-Object -First 1
		$childFromList = $list | where {$_.ResourceType -eq 'Microsoft.Sql/servers/databases'} | Select-Object -First 1

		# Assert
		Assert-AreEqual $expectedParent.Name $actualParent.Name
		Assert-AreEqual $expectedChild.Name $actualChild.Name
		Assert-AreEqual $expectedParent.ResourceType $actualParent.ResourceType
		Assert-AreEqual $expectedChild.ResourceType $actualChild.ResourceType

		Assert-AreEqual 2 $list.Count
		Assert-AreEqual $expectedParent.Name $parentFromList.Name
		Assert-AreEqual $expectedChild.Name $childFromList.Name
		Assert-AreEqual $expectedParent.ResourceType $parentFromList.ResourceType
		Assert-AreEqual $expectedChild.ResourceType $childFromList.ResourceType
	}
	Finally
	{
		# Cleanup
		Remove-AzureResourceGroup -Name $rgname -Force
	}
}