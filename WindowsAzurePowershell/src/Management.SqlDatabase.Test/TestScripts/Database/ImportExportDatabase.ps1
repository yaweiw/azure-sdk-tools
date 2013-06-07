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
    $UserName,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Password,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SubscriptionId,
    [Parameter(Mandatory=$true, Position=3)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SerializedCert,
    [Parameter(Mandatory=$true, Position=4)]
    [ValidateNotNullOrEmpty()]
    [Uri]
    $BlobContainerUri,
    [Parameter(Mandatory=$true, Position=5)]
    [ValidateNotNullOrEmpty()]
    [string]
    $StorageAccessKey,
    [Parameter(Mandatory=$true, Position=6)]
    [ValidateNotNullOrEmpty()]
    [String]
    $ServerLocation
)


$IsTestPass = $False

Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
Write-Output "`$SubscriptionId=$SubscriptionId"
Write-Output "`$SerializedCert=$SerializedCert"
Write-Output "`$BlobContainerUri=$BlobContainerUri"
Write-Output "`$StorageAccessKey=$StorageAccessKey"

. .\CommonFunctions.ps1

$ManageUrlPrefix = "https://"
$ManageUrlPostfix = ".database.windows.net/"

Try
{
    ####################################################
    # Set up test
	Init-TestEnvironment
    Init-AzureSubscription -SubscriptionID $SubscriptionId -SerializedCert $SerializedCert
    
    #create a server to use
    Write-Output "Creating server"
    $server = New-AzureSqlDatabaseServer -AdministratorLogin $UserName -AdministratorLoginPassword $Password `
        -Location $ServerLocation
    Assert {$server} "Failed to create a server"
    Write-Output "Server $($server.ServerName) created"
    
    #create a context to connect to the server.
    $ManageUrl = $ManageUrlPrefix + $server.ServerName + $ManageUrlPostfix
    $context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName `
        -Password $Password

    $DatabaseName = "testExportDatabase"
    
    Write-Output "Creating Database $DatabaseName ..."
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $DatabaseName
    Assert {$database} "Failed to create a database"
    Write-Output "Done"
    
    ####################################################
    # Export Database
    $BlobName = $DatabaseName + ".bacpac"
    $BlobUri = BlobContainerUri + $BlobName

    $requestId = Export-AzureSqlDatabase -UserName $UserName -Password $Password -ServerName $server.ServerName `
        -DatabaseName $DatabaseName -BlobUri $BlobUri -StorageKey $StorageAccessKey
    Assert {$requestId} "Failed to initiate the export opertaion"
    Write-Output "Request Id for export: " + $requestId

    $IsTestPass = $True

}
Finally
{
    if($database)
    {
        # Drop Database
        Drop-Database $Context $DatabaseName
        Drop-Server $server
        $Container = $BlobContainerUri.Segments[-1].Trim('/')
        Write-Output "Container: " + $Container
        
        #Remove-AzureStorageBlob -Blob $BlobUri
    }
}

Write-TestResult $IsTestPass
