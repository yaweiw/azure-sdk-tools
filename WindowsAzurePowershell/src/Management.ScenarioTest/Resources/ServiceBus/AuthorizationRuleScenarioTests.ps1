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

########################################################################### General Service Bus Scenario Tests ###########################################################################

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

########################################################################### New-AzureSBAuthorizationRule Scenario Tests ###########################################################################

<#
.SYNOPSIS
Test New-AzureSBAuthorizationRule when creating queue without passing any SAS keys.
#>
function Test-CreatesAuthorizationRuleWithoutKeys
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$entityType = "Queue"
	$client = New-ServiceBusClientExtensions
	$client.CreateQueue($namespaceName, $entityName)

	# Test
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $("Manage", "Send", "Listen")

	# Assert
	$expectedConnectionString = $client.GetConnectionString($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $expectedConnectionString $actual.ConnectionString
	Assert-AreEqual 3 $actual.Permission.Count
}

<#
.SYNOPSIS
Test New-AzureSBAuthorizationRule when creating topic with passing just primary key.
#>
function Test-CreatesAuthorizationRuleWithPrimaryKey
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$entityType = "Topic"
	$client = New-ServiceBusClientExtensions
	$client.CreateTopic($namespaceName, $entityName)
	$primaryKey = [Microsoft.ServiceBus.Messaging.SharedAccessAuthorizationRule]::GenerateRandomKey()

	# Test
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $("Listen") -PrimaryKey $primaryKey

	# Assert
	$expectedConnectionString = $client.GetConnectionString($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $expectedConnectionString $actual.ConnectionString
	Assert-AreEqual 1 $actual.Permission.Count
	Assert-AreEqual $primaryKey $actual.Rule.PrimaryKey
}

<#
.SYNOPSIS
Test New-AzureSBAuthorizationRule when creating relay with passing primary and secondary key.
#>
function Test-CreatesAuthorizationRuleWithPrimaryAndSecondaryKey
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$entityType = "Relay"
	$client = New-ServiceBusClientExtensions
	$client.CreateRelay($namespaceName, $entityName, "Http")
	$primaryKey = [Microsoft.ServiceBus.Messaging.SharedAccessAuthorizationRule]::GenerateRandomKey()
	$secondaryKey = [Microsoft.ServiceBus.Messaging.SharedAccessAuthorizationRule]::GenerateRandomKey()

	# Test
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $("Send", "Listen") -PrimaryKey $primaryKey -SecondaryKey $secondaryKey

	# Assert
	$expectedConnectionString = $client.GetConnectionString($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $expectedConnectionString $actual.ConnectionString
	Assert-AreEqual 2 $actual.Permission.Count
	Assert-AreEqual $primaryKey $actual.Rule.PrimaryKey
	Assert-AreEqual $secondaryKey $actual.Rule.SecondaryKey
}

<#
.SYNOPSIS
Test New-AzureSBAuthorizationRule on notification hub scope.
#>
function Test-CreatesAuthorizationRuleForNotificationHub
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$entityType = "NotificationHub"
	$client = New-ServiceBusClientExtensions
	$client.CreateNotificationHub($namespaceName, $entityName)

	# Test
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $("Send")

	# Assert
	$expectedConnectionString = $client.GetConnectionString($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $expectedConnectionString $actual.ConnectionString
	Assert-AreEqual 1 $actual.Permission.Count
}

<#
.SYNOPSIS
Test New-AzureSBAuthorizationRule on namespace scope.
#>
function Test-CreatesAuthorizationRuleForNamespace
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$ruleName = "myrule"
	$client = New-ServiceBusClientExtensions

	# Test
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -Permission $("Send")

	# Assert
	$expectedConnectionString = $client.GetConnectionString($namespaceName, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $expectedConnectionString $actual.ConnectionString
	Assert-AreEqual 1 $actual.Permission.Count
}

########################################################################### Set-AzureSBAuthorizationRule Scenario Tests ###########################################################################

<#
.SYNOPSIS
Test Set-AzureSBAuthorizationRule when creating queue and renewing primary key.
#>
function Test-SetsAuthorizationRuleRenewPrimaryKey
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$permission = $("Manage", "Send", "Listen")
	$entityType = "Queue"
	$client = New-ServiceBusClientExtensions
	$client.CreateQueue($namespaceName, $entityName)
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $permission
	$oldPrimaryKey = $actual.Rule.PrimaryKey

	# Test
	Set-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType

	# Assert
	$actual = $client.GetAuthorizationRule($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreNotEqual $oldPrimaryKey $actual.Rule.PrimaryKey
	Assert-AreEqualArray $permission $actual.Permission
}

<#
.SYNOPSIS
Test Set-AzureSBAuthorizationRule when creating topic and setting secondary key.
#>
function Test-SetsAuthorizationRuleSecondaryKey
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$entityType = "Topic"
	$permission = $("Manage", "Send", "Listen")
	$client = New-ServiceBusClientExtensions
	$client.CreateTopic($namespaceName, $entityName)
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $permission
	$oldSecondaryKey = $actual.Rule.SecondaryKey
	$newSecondaryKey = [Microsoft.ServiceBus.Messaging.SharedAccessAuthorizationRule]::GenerateRandomKey()

	# Test
	Set-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -SecondaryKey $newSecondaryKey

	# Assert
	$actual = $client.GetAuthorizationRule($namespaceName, $entityName, $entityType, $actual.Name)
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreEqual $newSecondaryKey $actual.Rule.SecondaryKey
	Assert-AreEqualArray $permission $actual.Permission
}

<#
.SYNOPSIS
Test Set-AzureSBAuthorizationRule when creating notification hub and changing the permissions.
#>
function Test-SetsAuthorizationRuleForPermission
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$entityName = "myentity"
	$ruleName = "myrule"
	$permission = $("Manage", "Send", "Listen")
	$entityType = "NotificationHub"
	$client = New-ServiceBusClientExtensions
	$client.CreateNotificationHub($namespaceName, $entityName)
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $permission
	$newPermission = $("Send")

	# Test
	Set-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -EntityName $entityName `
		-EntityType $entityType -Permission $newPermission

	# Assert
	$actual = $client.GetAuthorizationRule($namespaceName, $entityName, $entityType, $actual.Name)
	$actualPermission = $actual.Permission
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreNotEqual $oldPrimaryKey $actual.Rule.PrimaryKey
	Assert-AreEqualArray $newPermission $actualPermission
}

<#
.SYNOPSIS
Test Set-AzureSBAuthorizationRule on namespace level.
#>
function Test-SetsAuthorizationRuleOnNamespace
{
	# Setup
	Initialize-NamespaceTest
	New-Namespace 1
	$namespaceName = $createdNamespaces[0]
	$ruleName = "myrule"
	$permission = $("Manage", "Send", "Listen")
	$client = New-ServiceBusClientExtensions
	$actual = New-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -Permission $permission
	$newPermission = $("Send")

	# Test
	Set-AzureSBAuthorizationRule -Name $ruleName -Namespace $namespaceName -Permission $newPermission

	# Assert
	$actual = $client.GetAuthorizationRule($namespaceName, $actual.Name, "SharedAccessAuthorization")
	Assert-AreEqual $ruleName $actual.Name
	Assert-AreNotEqual $oldPrimaryKey $actual.Rule.PrimaryKey
	Assert-AreEqualArray $newPermission $actual.Permission
}