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

function TestImportWithRequestObject
{
    ####################################################
    # Import Database
	
	$status = $null
    
    ###########
	# Test the first parameter set

    $BlobName = $DatabaseName1 + ".bacpac"
	Write-Output "Importing from Blob: $BlobName"

	$Request = Start-AzureSqlDatabaseImport -SqlConnectionContext $context -StorageContainer $container `
		-DatabaseName $DatabaseName1 -BlobName $BlobName
    Assert {$Request} "Failed to initiate the first import opertaion"
	$id = ($Request.RequestGuid)
    Write-Output "Request Id for import1: $id"

    ##############
    # Test Get IE status with request object
	do
	{
		Start-Sleep -m 1500
		$status = Get-AzureSqlDatabaseImportExportStatus $Request
		$s = $status.Status
		Write-Output "Request Status: $s"
	} while($status.Status -ne "Completed")
}

function TestImportWithRequestObjectAndOptionalParameters
{
    ####################################################
    # Import Database
	$defaultCollation = "SQL_Latin1_General_CP1_CI_AS"
    $defaultIsReadOnly = $false
    $defaultIsFederationRoot = $false
    $defaultIsSystemObject = $false
    $defaultMaxSize = 1
    $defaultEdition = "Web"
	$status = $null
    $Edition = "Business"
    $MaxSize = 10
    
    ###########
	# Test optional Edition

    for( $i = 0; $i -ne 3; $i++)
    {
        Write-Output "Running test parameter set combination $i"
        $BlobName = $DatabaseName1 + ".bacpac"
        $dbName = $DatabaseName1 + "Options-$i"
	    Write-Output "\tImporting from Blob: $BlobName"

        $currEdition = $defaultEdition
        $currMaxSize = $defaultMaxSize

        if( ($i -eq 0) -or ($i -eq 2) )
        {
            $currEdition = $edition
        }
        elseif ($i -eq 1 -or ($i -eq 2) )
        {
            $currMaxSize = $MaxSize
        }

	    $Request = Start-AzureSqlDatabaseImport -SqlConnectionContext $context -StorageContainer $container `
		    -DatabaseName $dbName -BlobName $BlobName -Edition $currEdition -DatabaseMaxSize $MaxSize

        Assert {$Request} "\tFailed to initiate the second import opertaion"
	    $id = ($Request.RequestGuid)
        Write-Output "Request Id for import: $id"
    
        ##############
        # Test Get IE status with request object
	    do
	    {
		    Start-Sleep -m 1500
		    $status = Get-AzureSqlDatabaseImportExportStatus $Request
		    $s = $status.Status
		    Write-Output "Request Status: $s"
	    } while($status.Status -ne "Completed")

        $db = get-azuresqldatabase $context -DatabaseName $dbName
        Validate-SqlDatabase -Actual $db -ExpectedName $dbName -ExpectedCollationName $defaultCollation -ExpectedEdition `
                $currEdition -ExpectedMaxSizeGB $currMaxSize -ExpectedIsReadOnly $defaultIsReadOnly `
                -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
    }
}

function TestImportWithRequestId
{
    ###########
	# Test the second parameter set

    $BlobName2 = $DatabaseName2 + ".bacpac"
	Write-Output "Importing from Blob: $BlobName2"

	$Request = Start-AzureSqlDatabaseImport -SqlConnectionContext $context -StorageContext $StgCtx `
		-StorageContainerName $ContainerName -DatabaseName $DatabaseName2 -BlobName $BlobName2
    Assert {$Request} "Failed to initiate the third import opertaion"
	$id = ($Request.RequestGuid)
    Write-Output "Request Id for Import: $id"
    
    ##############
    # Test Get IE status with request id, servername, and login credentials
	do
	{
		Start-Sleep -m 1500
		$status = Get-AzureSqlDatabaseImportExportStatus -RequestId $Request.RequestGuid `
            -ServerName $server.ServerName -UserName $UserName -Password $Password
		$s = $status.Status
		Write-Output "Request Status: $s"
	} while($status.Status -ne "Completed")
}