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

$locations = Get-AzureSBLocation

$available = Test-AzureName -ServiceBusNamespace $name

if ($available)
{
    $serviceBusNamespaceObject = New-AzureSBNamespace $name $locations[$index].FullName

	do
	{
		$serviceBusNamespaceObject = $serviceBusNamespaceObject | Get-AzureSBNamespace
		Start-Sleep -s 1
	}
	while ($serviceBusNamespaceObject.Status -ne "Active")

	# Emit the service bus namespace object to the output pipeline
	Write-Output $serviceBusNamespaceObject

	# Remove the service bus namespace using piped object
	Remove-AzureSBNamespace $name
}
else
{
	Write-Error $("The namespace name (" + $name + ") is already used")
}