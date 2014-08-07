// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using Microsoft.Azure.Utilities.HttpRecorder;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Testing;


namespace Microsoft.WindowsAzure.Commands.ScenarioTest
{
    public class EnvironmentSetupHelper : IDisposable
    {
        private static string testEnvironmentName = "__test-environment";
        private WindowsAzureSubscription testSubscription;
        private TestEnvironment testEnvironment;
        protected List<string> modules;

        /// <summary>
        /// Loads DummyManagementClientHelper with clients and throws exception if any client is missing.
        /// </summary>
        /// <param name="initializedManagementClients"></param>
        public void SetupManagementClients(params object[] initializedManagementClients)
        {
            AzureSession.ClientFactoryInitializer = (p, a) => new DummyManagementClientHelper(initializedManagementClients);
        }

        /// <summary>
        /// Loads DummyManagementClientHelper with clients and sets it up to create missing clients dynamically.
        /// </summary>
        /// <param name="initializedManagementClients"></param>
        public void SetupSomeOfManagementClients(params object[] initializedManagementClients)
        {
            AzureSession.ClientFactoryInitializer = (p, a) => new DummyManagementClientHelper(initializedManagementClients, false);
        }

        public void SetupEnvironment(AzureModule mode)
        {
            // Ignore SSL errors
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            // Set RunningMocked
            if (HttpMockServer.GetCurrentMode() == HttpRecorderMode.Playback)
            {
                TestMockSupport.RunningMocked = true;
            }

            WindowsAzureProfile.Instance = new WindowsAzureProfile(new MockProfileStore());

            if (!WindowsAzureProfile.Instance.Environments.ContainsKey(testEnvironmentName))
            {
                WindowsAzureProfile.Instance.AddEnvironment(new WindowsAzureEnvironment { Name = testEnvironmentName });
            }

            SetupAzureEnvironmentFromEnvironmentVariables(mode);
        }

        private void SetupAzureEnvironmentFromEnvironmentVariables(AzureModule mode)
        {
            TestEnvironment rdfeEnvironment = new RDFETestEnvironmentFactory().GetTestEnvironment();
            TestEnvironment csmEnvironment = new CSMTestEnvironmentFactory().GetTestEnvironment();
            TestEnvironment currentEnvironment = (mode == AzureModule.AzureResourceManager
                ? csmEnvironment
                : rdfeEnvironment);

            string jwtToken;

            if (mode == AzureModule.AzureResourceManager)
            {
                jwtToken = csmEnvironment.Credentials != null ?
                ((TokenCloudCredentials)csmEnvironment.Credentials).Token : null;
            }
            else if (mode == AzureModule.AzureServiceManagement)
            {
                jwtToken = rdfeEnvironment.Credentials != null ?
                ((TokenCloudCredentials)rdfeEnvironment.Credentials).Token : null;
            }
            else
            {
                throw new ArgumentException("Invalid module mode.");
            }

            SetEndpointsToDefaults(rdfeEnvironment, csmEnvironment);

            WindowsAzureProfile.Instance.TokenProvider = new FakeAccessTokenProvider(jwtToken, csmEnvironment.UserName);

            WindowsAzureProfile.Instance.CurrentEnvironment = WindowsAzureProfile.Instance.Environments[testEnvironmentName];

            WindowsAzureProfile.Instance.CurrentEnvironment.ActiveDirectoryEndpoint =
                currentEnvironment.ActiveDirectoryEndpoint.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.GalleryEndpoint =
                currentEnvironment.GalleryUri.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.ResourceManagerEndpoint =
                csmEnvironment.BaseUri.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.ServiceEndpoint =
                rdfeEnvironment.BaseUri.AbsoluteUri;

            if (currentEnvironment.UserName == null)
            {
                currentEnvironment.UserName = "fakeuser@microsoft.com";
            }

            testSubscription = new WindowsAzureSubscription(false, false)
            {
                SubscriptionId = currentEnvironment.SubscriptionId,
                ActiveDirectoryEndpoint =
                    WindowsAzureProfile.Instance.CurrentEnvironment.ActiveDirectoryEndpoint,
                ActiveDirectoryUserId = currentEnvironment.UserName,
                SubscriptionName = currentEnvironment.SubscriptionId,
                ServiceEndpoint = new Uri(WindowsAzureProfile.Instance.CurrentEnvironment.ServiceEndpoint),
                ResourceManagerEndpoint = new Uri(WindowsAzureProfile.Instance.CurrentEnvironment.ResourceManagerEndpoint),
                TokenProvider = WindowsAzureProfile.Instance.TokenProvider,
                GalleryEndpoint = new Uri(WindowsAzureProfile.Instance.CurrentEnvironment.GalleryEndpoint),
                SqlDatabaseDnsSuffix = WindowsAzureProfile.Instance.CurrentEnvironment.SqlDatabaseDnsSuffix,
                CurrentStorageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT"),
                IsDefault = true
            };

            testEnvironment = currentEnvironment;
            if (HttpMockServer.GetCurrentMode() == HttpRecorderMode.Playback)
            {
                testSubscription.SetAccessToken(new FakeAccessToken
                {
                    AccessToken = "123",
                    UserId = testEnvironment.UserName
                });
            }
            else
            {
                testSubscription.SetAccessToken(WindowsAzureProfile.Instance.TokenProvider.GetNewToken(WindowsAzureProfile.Instance.CurrentEnvironment));
            }

            WindowsAzureProfile.Instance.AddSubscription(testSubscription);
            WindowsAzureProfile.Instance.Save();
        }

        private void SetEndpointsToDefaults(TestEnvironment rdfeEnvironment, TestEnvironment csmEnvironment)
        {
            // Set endpoints to default
            if (rdfeEnvironment.BaseUri == null)
            {
                rdfeEnvironment.BaseUri = new Uri(WindowsAzureEnvironmentConstants.AzureServiceEndpoint);
            }
            if (csmEnvironment.BaseUri == null)
            {
                csmEnvironment.BaseUri = new Uri(WindowsAzureEnvironmentConstants.AzureResourceManagerEndpoint);
            }
            if (rdfeEnvironment.GalleryUri == null)
            {
                rdfeEnvironment.GalleryUri = new Uri(WindowsAzureEnvironmentConstants.GalleryEndpoint);
            }
            if (csmEnvironment.GalleryUri == null)
            {
                csmEnvironment.GalleryUri = new Uri(WindowsAzureEnvironmentConstants.GalleryEndpoint);
            }
            if (rdfeEnvironment.ActiveDirectoryEndpoint == null)
            {
                rdfeEnvironment.ActiveDirectoryEndpoint = new Uri("https://login.windows.net/");
            }
            if (csmEnvironment.ActiveDirectoryEndpoint == null)
            {
                csmEnvironment.ActiveDirectoryEndpoint = new Uri("https://login.windows.net/");
            }
        }

        public void SetupModules(AzureModule mode, params string[] modules)
        {
            this.modules = new List<string>();
            if (mode == AzureModule.AzureServiceManagement)
            {
                this.modules.Add(@"ServiceManagement\Azure\Azure.psd1");
            }
            else if (mode == AzureModule.AzureResourceManager)
            {
                this.modules.Add(@"ResourceManager\AzureResourceManager\AzureResourceManager.psd1");
            }
            else
            {
                throw new ArgumentException("Unknown command type for testing");
            }
            this.modules.Add("Assert.ps1");
            this.modules.Add("Common.ps1");
            this.modules.AddRange(modules);
        }

        public virtual Collection<PSObject> RunPowerShellTest(params string[] scripts)
        {
            using (var powershell = System.Management.Automation.PowerShell.Create())
            {
                SetupPowerShellModules(powershell);

                Collection<PSObject> output = null;
                for (int i = 0; i < scripts.Length; ++i)
                {
                    Console.WriteLine(scripts[i]);
                    powershell.AddScript(scripts[i]);
                }
                try
                {
                    output = powershell.Invoke();

                    if (powershell.Streams.Error.Count > 0)
                    {
                        throw new RuntimeException(
                            "Test failed due to a non-empty error stream, check the error stream in the test log for more details.");
                    }

                    return output;
                }
                catch (Exception psException)
                {
                    powershell.LogPowerShellException(psException);
                    throw;
                }
                finally
                {
                    powershell.LogPowerShellResults(output);
                }
            }
        }

        private void SetupPowerShellModules(System.Management.Automation.PowerShell powershell)
        {
            powershell.AddScript(string.Format("cd \"{0}\"", Environment.CurrentDirectory));

            foreach (string moduleName in modules)
            {
                powershell.AddScript(string.Format("Import-Module \".\\{0}\"", moduleName));
            }

            powershell.AddScript("$VerbosePreference='Continue'");
            powershell.AddScript("$DebugPreference='Continue'");
            powershell.AddScript("$ErrorActionPreference='Stop'");
            powershell.AddScript("Write-Debug \"AZURE_TEST_MODE = $env:AZURE_TEST_MODE\"");
            powershell.AddScript("Write-Debug \"TEST_HTTPMOCK_OUTPUT = $env:TEST_HTTPMOCK_OUTPUT\"");
        }

        public void Dispose()
        {
            if (WindowsAzureProfile.Instance.Environments.ContainsKey(testEnvironmentName))
            {
                WindowsAzureProfile.Instance.RemoveEnvironment(testEnvironmentName);
            }
        }
    }
}
