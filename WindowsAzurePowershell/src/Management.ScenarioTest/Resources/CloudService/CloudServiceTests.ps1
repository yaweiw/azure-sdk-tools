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

########################################################################### General Cloud Service Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests any cloud based cmdlet with invalid credentials and expect it'll throw an exception.
#>
function Test-WithInvalidCredentials
{
	param([ScriptBlock] $cloudCmdlet)
	
	# Setup
	Remove-AllSubscriptions

	# Test
	Assert-Throws $cloudCmdlet "Call Set-AzureSubscription and Select-AzureSubscription first."
}

########################################################################### Remove-AzureService Scenario Tests ###########################################################################

<#
.SYNOPSIS
Tests Remove-AzureService with non-existing service.
#>
function Test-RemoveAzureServiceWithNonExistingService
{
	# Test
	Assert-Throws { Remove-AzureService "DoesNotExist" -Force } "The specified cloud service `"DoesNotExist`" does not exist."
}

<#
.SYNOPSIS
Tests Remove-AzureService with an existing service that does not have any deployments
#>
function Test-RemoveAzureServiceWithCloudService
{
	<# To Do: implement when we have unsigned version from Management.ServiceManagement assembly #>
}

<#
.SYNOPSIS
Tests Remove-AzureService with an existing service that has production deployment only
#>
function Test-RemoveAzureServiceWithProductionDeployment
{
	# Setup
	New-CloudService 1
	$name = $global:createdCloudServices[0]

	# Test
	$removed = Remove-AzureService $name -Force -PassThru

	# Assert
	Assert-True { $removed }
}

<#
.SYNOPSIS
Tests Remove-AzureService with an existing service that has staging deployment only
#>
function Test-RemoveAzureServiceWithStagingDeployment
{
	<# To Do: implement when we have unsigned version from Management.ServiceManagement assembly #>
}

<#
.SYNOPSIS
Tests Remove-AzureService with an existing service that has production and staging deployments
#>
function Test-RemoveAzureServiceWithFullCloudService
{
	<# To Do: implement when we have unsigned version from Management.ServiceManagement assembly #>
}

<#
.SYNOPSIS
Tests Remove-AzureService with WhatIf
#>
function Test-RemoveAzureServiceWhatIf
{
	# Setup
	New-CloudService 1
	$name = $global:createdCloudServices[0]

	# Test
	Remove-AzureService $name -Force -WhatIf
	$removed = Remove-AzureService $name -Force -PassThru

	# Assert
	Assert-True { $removed }
}

<#
.SYNOPSIS
Tests Remove-AzureService with WhatIf by passing invalid cloud service name and expects no error
#>
function Test-RemoveAzureServiceWhatIfWithInvalidName
{
	# Test
	Remove-AzureService "InvalidName" -Force -WhatIf

	# Assert
	Assert-True { $true }
}