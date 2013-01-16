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

$location = "North Central US"
$createdNamespaces = @()

<#
.SYNOPSIS
Gets valid and available service bus namespace name.
#>
function Get-NamespaceName
{
	do
	{
		$name = "OneSDK" + (Get-Random).ToString()
		$available = Test-AzureName -ServiceBusNamespace $name
	} while (!$available)

	return $name
}

<#
.SYNOPSIS
Removes given namespace when it's status is 'Active'

.PARAMETER name
The namespace name.
#>
function Remove-Namespace
{
	param([string]$name) 
	do
	{
		$namespace = Get-AzureSBNamespace $name
		Start-Sleep -s 1
	} while ($namespace.Status -ne "Active")

	Remove-AzureSBNamespace $name
}

<#
.SYNOPSIS
Clears the all created resources while doing the test.
#>
function Test-Cleanup
{
	foreach ($name in $createdNamespaces) { Remove-Namespace $name }
}