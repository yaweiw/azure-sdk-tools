# ----------------------------------------------------------------------------------
#
# Copyright 2011 Microsoft Corporation
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
    $Password
)

$IsTestPass = $False
Write-Output "`$Name=$Name"
Write-Output "`$ManageUrl=$ManageUrl"
Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
. .\CommonFunctions.ps1

Try
{
	Init-TestEnvironment
    $context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName -Password $Password
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $Name
    $edition = "Business"
    $maxSizeGB = "10"

    # Update with database object
    Write-Output "Updating Database $Name edition to $edition and maxSizeGB to $maxSizeGB ..."
    Set-AzureSqlDatabase $context $database -Edition $edition -MaxSizeGB $maxSizeGB -Force
    Write-Output "Done"

    $updatedDatabase = Get-AzureSqlDatabase $context $database.Name
    Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName $database.CollationName `
            -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly $database.IsReadOnly `
            -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject $database.IsSystemObject
    
    # Update with database name
    $edition = "Web"
    $maxSizeGB = "5"

    Write-Output "Updating Database $Name edition Back to $edition ..."
    Set-AzureSqlDatabase $context $database.Name -Edition $edition -MaxSizeGB $maxSizeGB -Force
    Write-Output "Done"

    $updatedDatabase = Get-AzureSqlDatabase $context $database
    Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $database.Name -ExpectedCollationName $database.CollationName `
            -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly $database.IsReadOnly `
            -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject $database.IsSystemObject
    
    #Rename a database
    $NewName = $Name + "-updated"

    Write-Output "Renaming a database from $Name to $NewName..."
    $updatedDatabase = Set-AzureSqlDatabase $context $database -NewName $NewName -PassThr -Force
    Write-Output "Done"
    Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName $database.CollationName `
            -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly $database.IsReadOnly `
            -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject $database.IsSystemObject

    $updatedDatabase = Get-AzureSqlDatabase $context -DatabaseName $NewName
    Validate-SqlDatabase -Actual $updatedDatabase -ExpectedName $NewName -ExpectedCollationName $database.CollationName `
            -ExpectedEdition $edition -ExpectedMaxSizeGB $maxSizeGB -ExpectedIsReadOnly $database.IsReadOnly `
            -ExpectedIsFederationRoot $database.IsFederationRoot -ExpectedIsSystemObject $database.IsSystemObject
    
    $getDroppedDatabase = Get-AzureSqlDatabase $context | Where-Object {$_.Name -eq $Name}
    Assert {!$getDroppedDatabase} "Database is not Renamed"
    
    $IsTestPass = $True
}
Finally
{
    if($database)
    {
        # Drop Database
        Drop-Databases $Context $Name
    }
}
Write-TestResult $IsTestPass
