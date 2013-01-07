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
    $Password
)

$IsTestPass = $False

Write-Output "`$Name=$Name"
Write-Output "`$ManageUrl=$ManageUrl"
Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
$NameStartWith = $Name
. .\CommonFunctions.ps1


Try
{
	Init-TestEnvironment
    $context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName -Password $Password

    $defaultCollation = "SQL_Latin1_General_CP1_CI_AS"
    $defaultEdition = "Web"
    $defaultMaxSizeGB = "1"
    $defaultIsReadOnly = $false
    $defaultIsFederationRoot = $false
    $defaultIsSystemObject = $false
    
    # Create Database with only required parameters
    Write-Output "Creating Database $Name ..."
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $Name
    Write-Output "Done"
    Validate-SqlDatabase -Actual $database -ExpectedName $Name -ExpectedCollationName $defaultCollation -ExpectedEdition `
            $defaultEdition -ExpectedMaxSizeGB $defaultMaxSizeGB -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
    
    #Get Database by database name
    $database = Get-AzureSqlDatabase -Context $context -DatabaseName $Name
    Validate-SqlDatabase -Actual $database -ExpectedName $Name -ExpectedCollationName $defaultCollation -ExpectedEdition `
            $defaultEdition -ExpectedMaxSizeGB $defaultMaxSizeGB -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
    
    # Create Database with all optional parameters
    $Name = $Name + "1"
    Write-Output "Creating Database $Name ..."
    $database2 = New-AzureSqlDatabase $context $Name -Collation "SQL_Latin1_General_CP1_CS_AS" -Edition "Business" `
            -MaxSizeGB 20 -Force
    Write-Output "Done"
    
    Validate-SqlDatabase -Actual $database2 -ExpectedName $Name -ExpectedCollationName "SQL_Latin1_General_CP1_CS_AS" `
            -ExpectedEdition "Business" -ExpectedMaxSizeGB "20" -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject

    #Get Database by database object
    $database2 = Get-AzureSqlDatabase -Context $context -Database $database2
    Validate-SqlDatabase -Actual $database2 -ExpectedName $Name -ExpectedCollationName "SQL_Latin1_General_CP1_CS_AS" `
            -ExpectedEdition "Business" -ExpectedMaxSizeGB "20" -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
            
    #Get Databases with no filter
    $databases = Get-AzureSqlDatabase -Context $context | Where-Object {$_.Name.StartsWith($NameStartWith)}
    Assert {$databases.Count -eq 2} "Get database should have returned 2 database, but returned $databases.Count"
    
    $IsTestPass = $True
}
Finally
{
    if($database)
    {
        # Drop Database
        Drop-Databases $Context $NameStartWith
    }
}

Write-TestResult $IsTestPass
