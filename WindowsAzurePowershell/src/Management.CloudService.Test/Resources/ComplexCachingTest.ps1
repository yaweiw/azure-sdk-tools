# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http:#www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

$available = Test-AzureName -Service $cloudService

if ($available)
{
	New-AzureServiceProject $cloudService

	Add-AzureNodeWebRole WebRole

	Add-AzureNodeWorkerRole WorkerRole1

	Add-AzureCacheWorkerRole CacheWorkerRole1

	Add-AzureCacheWorkerRole CacheWorkerRole2

	Enable-AzureMemcacheRole WebRole CacheWorkerRole1

	Enable-AzureMemcacheRole WorkerRole1 CacheWorkerRole2

	$deployment = Publish-AzureServiceProject -Force

	$removed = Remove-AzureService -Force -PassThru

	if (!$removed)
	{
		Write-Error $("Removing cloud service (" + $cloudService + ") failed")
	}
}
else
{
	Write-Error $("The service name (" + $cloudService + ") is already used")
}