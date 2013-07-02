# ----------------------------------------------------------------------------------
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

function TestExportWithRequestObject
{
    ####################################################
    # Export Database
	
	$status = $null
    
    ###########
	# Test the first parameter set

    $BlobName = $DatabaseName1 + ".bacpac"
	Write-Output "Exporting to Blob:  $BlobName"

	$Request = Start-AzureSqlDatabaseExport -SqlConnectionContext $context -StorageContainer $container `
		-DatabaseName $DatabaseName1 -BlobName $BlobName
    Assert {$Request} "Failed to initiate the first export opertaion"
	$id = ($Request.RequestGuid)
    Write-Output "Request Id for export1: $id"
    
    ##############
    # Test Get IE status with request object
	do
	{
		Start-Sleep -m 1500
		$status = Get-AzureSqlDatabaseImportExportStatus $Request
		$s = $status.Status
		Write-Output "Request1 Status: $s"
	} while($status.Status -ne "Completed")
}

function TestExportWithRequestId
{
    ###########
	# Test the second parameter set

    $BlobName2 = $DatabaseName2 + ".bacpac"
	Write-Output "Exporting to Blob: $BlobName2"

	$Request2 = Start-AzureSqlDatabaseExport -SqlConnectionContext $context -StorageContext $StgCtx `
		-StorageContainerName $ContainerName -DatabaseName $DatabaseName2 -BlobName $BlobName2
    Assert {$Request2} "Failed to initiate the second export opertaion"
	$id = ($Request2.RequestGuid)
    Write-Output "Request Id for export2: $id"
    
    ##############
    # Test Get IE status with request id, server name, and login credentials
	do
	{
		Start-Sleep -m 1500
		$status = Get-AzureSqlDatabaseImportExportStatus -RequestId $Request2.RequestGuid `
            -ServerName $server.ServerName -UserName $UserName -Password $Password
		$s = $status.Status
		Write-Output "Request2 Status: $s"
	} while($status.Status -ne "Completed")
}