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

.".\\Common.ps1"
.".\\Assert.ps1"
.".\\Websites\\Common.ps1"
.".\\Websites\\WebsitesTests.ps1"
$global:totalCount = 0;
$global:passedCount = 0;
Add-Type -Path ".\Microsoft.Azure.Utilities.HttpRecorder.dll"
[Microsoft.Azure.Utilities.HttpRecorder.HttpMockServer]::Initialize("foo", "bar")
function Run-TestProtected
{
   param([ScriptBlock]$script, [string] $testName)
   try 
   {
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host  -ForegroundColor Green "Running test $testName"
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host
     &$script
	 $global:passedCount = $global:passedCount + 1
	 Write-Host
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host -ForegroundColor Green "Test Passed"
     Write-Host  -ForegroundColor Green =====================================
	 Write-Host
   }
   catch
   {
     Out-String -InputObject $_.Exception | Write-Host -ForegroundColor Red
	 Write-Host
     Write-Host  -ForegroundColor Red =====================================
	 Write-Host -ForegroundColor Red "Test Failed"
     Write-Host  -ForegroundColor Red =====================================
	 Write-Host
   }
   finally
   {
      $global:totalCount = $global:totalCount + 1
   }
}

Write-Host Initializing websites tests
Initialize-WebsiteTest
Write-Host Initialization Completed

Run-TestProtected {Run-Test {"Test-GettingJobHistory"}} "Test-GettingJobHistory"
Run-TestProtected {Run-Test {"Test-GettingWebsiteJobs"}} "Test-GettingWebsiteJobs"
Run-TestProtected {Run-Test {"Test-StartAndStopAzureWebsiteContinuousJob"}} "Test-StartAndStopAzureWebsiteContinuousJob"
Run-TestProtected {Run-Test {"Test-StartAzureWebsiteTriggeredJob"}} "Test-StartAzureWebsiteTriggeredJob"
Run-TestProtected {Run-Test {"Test-RemoveNonExistingAzureWebsiteJob"}} "Test-RemoveNonExistingAzureWebsiteJob"
Run-TestProtected {Run-Test {"Test-RemoveAzureWebsiteContinuousJob"}} "Test-RemoveAzureWebsiteContinuousJob"
Run-TestProtected {Run-Test {"Test-RemoveAzureWebsiteTriggeredJob"}} "Test-RemoveAzureWebsiteTriggeredJob"
Run-TestProtected {Run-Test {"Test-SetAzureWebsite"}} "Test-SetAzureWebsite"
Run-TestProtected {Run-Test {"Test-NewAzureWebSiteUpdateGit"}} "Test-NewAzureWebSiteUpdateGit"
Run-TestProtected {Run-Test {"Test-NewAzureWebSiteGitHubAllParms"}} "Test-NewAzureWebSiteGitHubAllParms"
Run-TestProtected {Run-Test {"Test-NewAzureWebSiteMultipleCreds"}} "Test-NewAzureWebSiteMultipleCreds"
Run-TestProtected {Run-Test {"Test-AzureWebSiteShowSingleSite"}} "Test-AzureWebSiteShowSingleSite"
Run-TestProtected {Run-Test {"Test-AzureWebSiteListAll"}} "Test-AzureWebSiteListAll"
Run-TestProtected {Run-Test {"Test-GetAzureWebSiteListNone"}} "Test-GetAzureWebSiteListNone"
Run-TestProtected {Run-Test {"Test-KuduAppsExpressApp"}} "Test-KuduAppsExpressApp"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteLocation"}} "Test-GetAzureWebsiteLocation"
Run-TestProtected {Run-Test {"Test-DisablesBothByDefault"}} "Test-DisablesBothByDefault"
Run-TestProtected {Run-Test {"Test-DisablesStorageOnly"}} "Test-DisablesStorageOnly"
Run-TestProtected {Run-Test {"Test-DisablesFileOnly"}} "Test-DisablesFileOnly"
Run-TestProtected {Run-Test {"Test-DisableApplicationDiagnosticOnTableStorageAndFile"}} "Test-DisableApplicationDiagnosticOnTableStorageAndFile"
Run-TestProtected {Run-Test {"Test-DisableApplicationDiagnosticOnFileSystem"}} "Test-DisableApplicationDiagnosticOnFileSystem"
Run-TestProtected {Run-Test {"Test-DisableApplicationDiagnosticOnTableStorage"}} "Test-DisableApplicationDiagnosticOnTableStorage"
Run-TestProtected {Run-Test {"Test-ThrowsForInvalidStorageAccountName"}} "Test-ThrowsForInvalidStorageAccountName"
Run-TestProtected {Run-Test {"Test-ReconfigureStorageAppDiagnostics"}} "Test-ReconfigureStorageAppDiagnostics"
Run-TestProtected {Run-Test {"Test-UpdateTheDiagnositicLogLevel"}} "Test-UpdateTheDiagnositicLogLevel"
Run-TestProtected {Run-Test {"Test-EnableApplicationDiagnosticOnFileSystem"}} "Test-EnableApplicationDiagnosticOnFileSystem"
Run-TestProtected {Run-Test {"Test-EnableApplicationDiagnosticOnTableStorage"}} "Test-EnableApplicationDiagnosticOnTableStorage"
Run-TestProtected {Run-Test {"Test-RestartAzureWebsite"}} "Test-RestartAzureWebsite"
Run-TestProtected {Run-Test {"Test-StopAzureWebsite"}} "Test-StopAzureWebsite"
Run-TestProtected {Run-Test {"Test-StartAzureWebsite"}} "Test-StartAzureWebsite"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteWithStoppedSite"}} "Test-GetAzureWebsiteWithStoppedSite"
Run-TestProtected {Run-Test {"Test-GetAzureWebsite"}} "Test-GetAzureWebsite"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteLogListPath"}} "Test-GetAzureWebsiteLogListPath"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteLogTailUriEncoding"}} "Test-GetAzureWebsiteLogTailUriEncoding"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteLogTailPath"}} "Test-GetAzureWebsiteLogTailPath"
Run-TestProtected {Run-Test {"Test-GetAzureWebsiteLogTail"}} "Test-GetAzureWebsiteLogTail"
Run-TestProtected {Run-Test {Test-RemoveAzureServiceWithWhatIf}} "Test-RemoveAzureServiceWithWhatIf"
Run-TestProtected {Run-Test {Test-RemoveAzureServiceWithNonExistingName}} "Test-RemoveAzureServiceWithNonExistingName"
Run-TestProtected {Run-Test {Test-RemoveAzureServiceWithValidName}} "Test-RemoveAzureServiceWithValidName"
Run-TestProtected {Run-Test {Test-WithInvalidCredentials { Get-AzureWebsiteLog -Tail -Name $(Get-WebsiteName) }}} "TestGetAzureWebsiteLogWithInvalidCredentials"
Run-TestProtected {Run-Test {Test-WithInvalidCredentials {Remove-AzureWebsite $(Get-WebsiteName) -Force }}} "TestRemoveAzureWebsiteWithInvalidCredentials"
Write-Host
Write-Host -ForegroundColor Green "$global:passedCount / $global:totalCount Website Tests Pass"
Write-Host

