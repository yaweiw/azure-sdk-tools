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
    $Password,
    [Parameter(Mandatory=$true, Position=4)]
    [ValidateNotNullOrEmpty()]
    [String]
    $OutputFile
)

$IsTestPass = $False

Write-Output "`$Name=$Name"
Write-Output "`$ManageUrl=$ManageUrl"
Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
Write-Output "`$OutputFile=$OutputFile"
. .\CommonFunctions.ps1


Try
{
	Init-TestEnvironment
    $context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName -Password $Password
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $Name
    
    # write output object to output file
    $context.GetType().Name >> $OutputFile
    $context | ft -Wrap -AutoSize >> $OutputFile
    $context | fl >> $OutputFile

    $database.GetType().Name >> $OutputFile
    $Database | ft -Wrap -AutoSize >> $OutputFile
    $database | fl >> $OutputFile
    
    $isTestPass = $True
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

