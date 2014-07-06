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
Param( [switch] $Record )

$scriptFolder = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ($scriptFolder + '.\SetupTestEnv.ps1')

# Access key is long and impossible to remember, so we don't ask from the command line rather just require environment variables.
$resourceManagerVariables = Test-Path env:TEST_CSM_ORGID_AUTHENTICATION
$serviceManagementVariables = Test-Path env:TEST_ORGID_AUTHENTICATION
$oldRdfeTestVariables = $(Test-Path env:AZURE_STORAGE_ACCESS_KEY) -and $(Test-Path env:AZURE_STORAGE_ACCOUNT)
if (!$serviceManagementVariables -AND !$resourceManagerVariables -AND !$oldRdfeTestVariables) {
  #TODO rewording the help information
  Write-Host "For Service Management please set environment variables 'AZURE_STORAGE_ACCESS_KEY' and 'AZURE_STORAGE_ACCOUNT' and for Resource Manage set TEST_CSM_ORGID_AUTHENTICATION" -ForegroundColor "Red"

  $subscription = Read-Host 'Please input the azure subscription guid you tests will use'
  $env:TEST_ORGID_AUTHENTICATION = "SubscriptionId=$subscription;BaseUri=https://management.core.windows.net/;AADAuthEndpoint=https://login.windows.net/"
  $env:TEST_CSM_ORGID_AUTHENTICATION = "SubscriptionId=$subscription;BaseUri=https://management.azure.com/;AADAuthEndpoint=https://login.windows.net/"

  Write-Host "$env:TEST_ORGID_AUTHENTICATION"
  Write-Host "$env:TEST_CSM_ORGID_AUTHENTICATION"
  #TODO: find a way to persist the ids, and tell user so, and also how to set for godfood azure environments

  #throw "Missing environment variables" 
}

$env:AZURE_TEST_ENVIRONMENT="production"

if ($Record) {
	Write-Host "Setting up 'Record' mode"
	$env:AZURE_TEST_MODE="Record"
	$env:TEST_HTTPMOCK_OUTPUT="$env:AzurePSRoot\src\Common\Commands.ScenarioTest\Resources\SessionRecords\"
	Write-Host "The HTTP traffic will be captured under $env:TEST_HTTPMOCK_OUTPUT." -ForegroundColor "Green"
}

Write-Host "Environment has been set up. You can launch Visual Studio to run tests by typing devenv.exe here; Or through msbuild.exe" -ForegroundColor "Green"
