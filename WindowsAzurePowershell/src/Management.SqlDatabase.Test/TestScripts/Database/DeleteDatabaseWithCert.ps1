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
Write-Output "`$ServerName=$ServerName"
Write-Output "`$SubscriptionID=$SubscriptionID"
Write-Output "`$SerializedCert=$SerializedCert"
Write-Output "`$Endpoint=$Endpoint"
. .\CommonFunctions.ps1

Try
{
	Init-TestEnvironment
	Init-AzureSubscription $SubscriptionId $SerializedCert $Endpoint

    $context = Get-ServerContextByServerNameWithCertAuth $ServerName

    $database = New-AzureSqlDatabase -Context $context -DatabaseName $Name
    
	############################################################
    # Delete database by pasing database object
    Write-Output "Deleting Database by passing Database object ..."
    Remove-AzureSqlDatabase $context $database -Force
    Write-Output "Done"
    
    $getDroppedDatabase = Get-AzureSqlDatabase -ConnectionContext $context | Where-Object {$_.Name -eq $Name}
    Assert {!$getDroppedDatabase} "Database is not dropped"
    
	############################################################
    # Delete database by pasing database name
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $Name
    Write-Output "Deleting Database by passing Database Name ..."
    Remove-AzureSqlDatabase $context $database.Name -Force
    Write-Output "Done"
    
    $getDroppedDatabase = Get-AzureSqlDatabase -ConnectionContext $context | Where-Object {$_.Name -eq $Name}
    Assert {!$getDroppedDatabase} "Database is not dropped"    
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
