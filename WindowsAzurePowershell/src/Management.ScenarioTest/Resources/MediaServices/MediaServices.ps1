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

########################################################################### General Scenario Tests ###########################################################################

function EnsureTestAccountExists
{
	$accounts = Get-AzureMediaServices

	Foreach($account in $accounts)
	{
		if ($account.Name -eq $MediaAccountName) 
		{ 
			return
		}
	}
	New-AzureMediaServices -MediaServicesAccountName $MediaServicesAccountName -Location $Region -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey -BlobStorageEndpointUri $BlobStorageEndpointUri
}

<#
.SYNOPSIS
Tests rotate key.
#>
function Test-NewAzureMediaServicesKey
{
	EnsureTestAccountExists

	$key = New-AzureMediaServicesAccountKey -MediaServicesAccountName $MediaServicesAccountName Secondary -Force

	$account = Get-AzureMediaServices -MediaServicesAccountName $MediaServicesAccountName

	Assert-AreEqual $key $account.SecondaryAccountKey
}

<#
.SYNOPSIS
Tests delete account.
#>
function Test-RemoveAzureMediaServicesAccount
{
	EnsureTestAccountExists

	Remove-AzureMediaServicesAccount -MediaServicesAccountName $MediaServicesAccountName -Force

	#Assert-Throws {Get-AzureMediaServices -MediaServicesAccountName $MediaServicesAccountName}
}
