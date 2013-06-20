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

[CmdletBinding()]
Param
(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Name,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]
    $ManageUrl,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]
    $UserName,
    [Parameter(Mandatory=$true, Position=3)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Password,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]
    $ServerName,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SubscriptionID,
    [Parameter(Mandatory=$true, Position=3)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SerializedCert,
    [Parameter(Mandatory=$true, Position=4)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Endpoint
)

$IsTestPass = $False
Write-Output "`$Name=$Name"
Write-Output "`$ManageUrl=$ManageUrl"
Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
Write-Output "`$ServerName=$ServerName"
Write-Output "`$SubscriptionID=$SubscriptionID"
Write-Output "`$SerializedCert=$SerializedCert"
Write-Output "`$Endpoint=$Endpoint"

. .\CommonFunctions.ps1

function Scenario1-UpdateWithObject
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$false, Position=0)]
        [ValidateNotNullOrEmpty()]
        [Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server.IServerDataServiceContext]
        $Context,

		[Parameter(Mandatory=$false, Position=1)]
        [ValidateNotNullOrEmpty()]
        [String]
        $ServerName
	)
	
	$edition = "Business"
	$maxSizeGB = "10"

	Write-Output "Starting Test Scenario 1"

	if($Context)
	{
		Write-Output "Updating Database $Name edition to $edition and maxSizeGB to $maxSizeGB ..."
		Set-AzureSqlDatabase $context $database -Edition $edition -MaxSizeGB $maxSizeGB -Force
		Write-Output "Done"

		$updatedDatabase = Get-AzureSqlDatabase $context -DatabaseName $database.Name
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName `
			$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
			$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
			$database.IsSystemObject
	}
	elseif ($serverName)
	{
		Write-Output "Updating Database $Name edition to $edition and maxSizeGB to $maxSizeGB ..."
		Set-AzureSqlDatabase -ServerName $ServerName $database -Edition $edition -MaxSizeGB $maxSizeGB -Force
		Write-Output "Done"

		$updatedDatabase = Get-AzureSqlDatabase -ServerName $ServerName -DatabaseName $database.Name
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName `
			$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
			$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
			$database.IsSystemObject
	}
}

function Scenario2-UpdateWithName
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$false, Position=0)]
        [ValidateNotNullOrEmpty()]
        [Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server.IServerDataServiceContext]
        $Context,

		[Parameter(Mandatory=$false, Position=1)]
        [ValidateNotNullOrEmpty()]
        [String]
        $ServerName
	)
	
	$edition = "Web"
	$maxSizeGB = "5"
	
	Write-Output "Starting Test Scenario 2"

	if($Context)
	{
		Write-Output "Updating Database $Name edition Back to $edition ..."
		Set-AzureSqlDatabase $context $database.Name -Edition $edition -MaxSizeGB $maxSizeGB -Force
		Write-Output "Done"

		$updatedDatabase = Get-AzureSqlDatabase $context -Database $database
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName `
				$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
				$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
				$database.IsSystemObject
	}
	elseif ($serverName)
	{
		Write-Output "Updating Database $Name edition Back to $edition ..."
		Set-AzureSqlDatabase -ServerName $ServerName $database.Name -Edition $edition -MaxSizeGB $maxSizeGB -Force
		Write-Output "Done"

		$updatedDatabase = Get-AzureSqlDatabase -ServerName $ServerName -Database $database
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName `
			$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
			$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
			$database.IsSystemObject
	}
}

function Scenario3-RenameDatabase
{
	[CmdletBinding()]
	param
	(
		[Parameter(Mandatory=$false, Position=0)]
        [ValidateNotNullOrEmpty()]
        [Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server.IServerDataServiceContext]
        $Context,

		[Parameter(Mandatory=$false, Position=1)]
        [ValidateNotNullOrEmpty()]
        [String]
        $ServerName
	)
	
	Write-Output "Starting Test Scenario 3"

	$NewName = $Name + "-updated"

	if($Context)
	{
		Write-Output "Renaming a database from $Name to $NewName..."
		$updatedDatabase = Set-AzureSqlDatabase $context $database -NewName $NewName -PassThr -Force
		Write-Output "Done"
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName `
				$database.CollationName -ExpectedEdition $database.Edition -ExpectedMaxSizeGB $database.MaxSizeGB `
				-ExpectedIsReadOnly $database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot `
				-ExpectedIsSystemObject $database.IsSystemObject

		$updatedDatabase = Get-AzureSqlDatabase $context -DatabaseName $NewName
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName `
				$database.CollationName -ExpectedEdition $database.Edition -ExpectedMaxSizeGB $database.MaxSizeGB `
				-ExpectedIsReadOnly $database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot `
				-ExpectedIsSystemObject $database.IsSystemObject
    
		$database = Get-AzureSqlDatabase $context | Where-Object {$_.Name -eq $Name}
		Assert {!$getDroppedDatabase} "Database is not Renamed"
	}
	elseif ($serverName)
	{
		Write-Output "Renaming a database from $Name to $NewName..."
		$updatedDatabase = Set-AzureSqlDatabase -ServerName $ServerName $database -NewName $NewName -PassThr -Force
		Write-Output "Done"

		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName `
			$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
			$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
			$database.IsSystemObject

		$updatedDatabase = Get-AzureSqlDatabase -ServerName $ServerName -DatabaseName $NewName
		Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName `
			$database.CollationName -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly `
			$database.IsReadOnly -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject `
			$database.IsSystemObject
	}
}

Try
{
	Init-TestEnvironment
    # Update with Sql Auth
    try
	{
		$context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName `
			-Password $Password
			
		$database = New-AzureSqlDatabase -Context $context -DatabaseName $Name

		Scenario1-UpdateWithObject -Context $context

		Scenario2-UpdateWithName -Context $context
		
		Scenario3-RenameDatabase -Context $context
	}
	finally
	{
		# Drop Database
		Drop-Databases $Context $Name
	}

	# Update with Cert Auth
    try
	{
		Init-AzureSubscription $SubscriptionId $SerializedCert $Endpoint
		$sub = Get-AzureSubscription -Current

		$context = New-AzureSqlDatabaseServerContext -ServerName $ServerName -UseSubscription
			
		$database = New-AzureSqlDatabase -Context $context -DatabaseName $Name

		Scenario1-UpdateWithObject -Context $context

		Scenario2-UpdateWithName -Context $context
		
		Scenario3-RenameDatabase -Context $context
	}
	finally
	{
		# Drop Database
		Drop-Databases $Context $Name
		Remove-AzureSubscription $sub.SubscriptionName -Force
	}
	

	# Update with Cert Auth with Server Name
    try
	{
		Init-AzureSubscription $SubscriptionId $SerializedCert $Endpoint
		$sub = Get-AzureSubscription -Current

		$database = New-AzureSqlDatabase -ServerName $context -DatabaseName $Name

		Scenario1-UpdateWithObject -ServerName $ServerName

		Scenario2-UpdateWithName -ServerName $ServerName
		
		Scenario3-RenameDatabase -ServerName $ServerName
	}
	finally
	{
		# Drop Database
		Drop-Databases $Context $Name
		Remove-AzureSubscription $sub.SubscriptionName -Force
	}


    $IsTestPass = $True
}
Finally
{
    Drop-Databases $Context $Name
}
Write-TestResult $IsTestPass
