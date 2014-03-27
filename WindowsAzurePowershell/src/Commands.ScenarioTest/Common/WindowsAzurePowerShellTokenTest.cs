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


using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.Common
{
    using System;
    using System.Collections.Generic;
    using VisualStudio.TestTools.UnitTesting;
    using Commands.Common;
    using Microsoft.WindowsAzure.Utilities.HttpRecorder;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [TestClass]
    public class WindowsAzurePowerShellTokenTest : PowerShellTest
    {
        protected List<HttpMockServer> mockServers;
        private string userName;
        private static string testEnvironmentName = "__test-environment";

        private void OnClientCreated(object sender, ClientCreatedArgs e)
        {
            HttpMockServer mockServer = new HttpMockServer(new SimpleRecordMatcher());
            e.AddHandlerToClient(mockServer);
            mockServers.Add(mockServer);
        }

        public WindowsAzurePowerShellTokenTest(params string[] modules)
            : base(modules)
        { }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            this.mockServers = new List<HttpMockServer>();
            WindowsAzureSubscription.OnClientCreated += OnClientCreated;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) =>
            {
                return true;
            };

            ServiceManagementTestEnvironmentFactory serviceManagementTestEnvironmentFactory = new ServiceManagementTestEnvironmentFactory();
            TestEnvironment rdfeEnvironment = serviceManagementTestEnvironmentFactory.GetTestEnvironment();
            ResourceManagerTestEnvironmentFactory resourceManagerTestEnvironmentFactory = new ResourceManagerTestEnvironmentFactory();
            TestEnvironment csmEnvironment = resourceManagerTestEnvironmentFactory.GetTestEnvironment();
            string jwtToken = ((TokenCloudCredentials)csmEnvironment.Credentials).Token;

            WindowsAzureProfile.Instance.TokenProvider = new FakeAccessTokenProvider(jwtToken, csmEnvironment.UserName);

            userName = csmEnvironment.UserName;

            CreateTestAzureEnvironment(csmEnvironment, rdfeEnvironment);
        }

        private void CreateTestAzureEnvironment(TestEnvironment csmEnvironment, TestEnvironment rdfeEnvironment)
        {
            if (!WindowsAzureProfile.Instance.Environments.ContainsKey(testEnvironmentName))
            {
                WindowsAzureProfile.Instance.AddEnvironment(new WindowsAzureEnvironment {Name = testEnvironmentName});
            }

            WindowsAzureProfile.Instance.CurrentEnvironment = WindowsAzureProfile.Instance.Environments[testEnvironmentName];

            WindowsAzureProfile.Instance.CurrentEnvironment.ActiveDirectoryEndpoint =
                csmEnvironment.ActiveDirectoryEndpoint.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.GalleryEndpoint =
                csmEnvironment.GalleryUri.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.ResourceManagerEndpoint =
                csmEnvironment.BaseUri.AbsoluteUri;
            WindowsAzureProfile.Instance.CurrentEnvironment.ServiceEndpoint =
                rdfeEnvironment.BaseUri.AbsoluteUri;

            RunPowerShellTest("Add-AzureAccount -Environment " + testEnvironmentName);

            foreach (var subscription in WindowsAzureProfile.Instance.Subscriptions)
            {
                subscription.TokenProvider = WindowsAzureProfile.Instance.TokenProvider;
                if (subscription.ActiveDirectoryUserId == userName)
                {
                    subscription.IsDefault = true;
                }
                else
                {
                    subscription.IsDefault = false;
                }
            }
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
            WindowsAzureSubscription.OnClientCreated -= OnClientCreated;
            mockServers.ForEach(ms => ms.Dispose());

            WindowsAzureProfile.Instance.RemoveEnvironment(testEnvironmentName);
        }
    }
}