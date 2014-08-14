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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Xunit;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    public class ProfileClientTests
    {
        private string oldProfileData;
        private string oldProfileDataPath;
        private string newProfileDataPath;
        private string defaultSubscription = "06E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E";
        private WindowsAzure.Subscriptions.Models.SubscriptionListOperationResponse.Subscription rdfeSubscription1;
        private WindowsAzure.Subscriptions.Models.SubscriptionListOperationResponse.Subscription rdfeSubscription2;
        private Azure.Subscriptions.Models.Subscription csmSubscription1;
        private Azure.Subscriptions.Models.Subscription csmSubscription2;
        private AzureSubscription azureSubscription1;
        private AzureSubscription azureSubscription2;
        private AzureSubscription azureSubscription3withoutUser;
        private AzureEnvironment azureEnvironment;
        
        [Fact]
        public void ProfileGetsCreatedWithNonExistingFile()
        {
            ProfileClient.DataStore = new MockDataStore();
            ProfileClient client = new ProfileClient();
        }

        [Fact]
        public void ProfileMigratesOldData()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            dataStore.VirtualStore[oldProfileDataPath] = oldProfileData;
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();

            Assert.False(dataStore.FileExists(oldProfileDataPath));
            Assert.True(dataStore.FileExists(newProfileDataPath));
        }

        [Fact]
        public void ProfileMigratesOldDataOnce()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            dataStore.VirtualStore[oldProfileDataPath] = oldProfileData;
            ProfileClient.DataStore = dataStore;
            ProfileClient client1 = new ProfileClient();

            Assert.False(dataStore.FileExists(oldProfileDataPath));
            Assert.True(dataStore.FileExists(newProfileDataPath));

            ProfileClient client2 = new ProfileClient();

            Assert.False(dataStore.FileExists(oldProfileDataPath));
            Assert.True(dataStore.FileExists(newProfileDataPath));
        }

        [Fact]
        public void ProfileLoadsOldData()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            dataStore.VirtualStore[oldProfileDataPath] = oldProfileData;
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            var testSubscription = client.Profile.Subscriptions[new Guid(defaultSubscription)];

            Assert.False(dataStore.FileExists(oldProfileDataPath));
            Assert.True(dataStore.FileExists(newProfileDataPath));
            Assert.Equal(2, client.Profile.Subscriptions.Count);
            Assert.Equal(4, client.Profile.Environments.Count);
            Assert.Equal("Test", testSubscription.Name);
            Assert.Equal(EnvironmentName.AzureCloud, testSubscription.Environment);
        }

        [Fact]
        public void AddAzureAccountReturnsAccountWithSubscriptionsInRdfeMode()
        {
            SetMockData();
            SetMocks(new[] { rdfeSubscription1, rdfeSubscription2 }.ToList(), new[] { csmSubscription1 }.ToList());
            MockDataStore dataStore = new MockDataStore();
            dataStore.VirtualStore[oldProfileDataPath] = oldProfileData;
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureServiceManagement;

            var account = client.AddAzureAccount(new Utilities.Common.Authentication.UserCredentials { UserName = "test" }, EnvironmentName.AzureCloud);

            Assert.Equal("test", account.UserName);
            Assert.Equal(2, account.Subscriptions.Count);
            Assert.True(account.Subscriptions.Any(s => s.Id == new Guid(rdfeSubscription1.SubscriptionId)));
            Assert.True(account.Subscriptions.Any(s => s.Id == new Guid(rdfeSubscription2.SubscriptionId)));
        }

        [Fact]
        public void AddAzureAccountReturnsAccountWithSubscriptionsInCsmMode()
        {
            SetMockData();
            SetMocks(new[] { rdfeSubscription1, rdfeSubscription2 }.ToList(), new[] { csmSubscription1 }.ToList());
            MockDataStore dataStore = new MockDataStore();
            dataStore.VirtualStore[oldProfileDataPath] = oldProfileData;
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;

            var account = client.AddAzureAccount(new Utilities.Common.Authentication.UserCredentials { UserName = "test" }, EnvironmentName.AzureCloud);

            Assert.Equal("test", account.UserName);
            Assert.Equal(3, account.Subscriptions.Count);
            Assert.True(account.Subscriptions.Any(s => s.Id == new Guid(rdfeSubscription1.SubscriptionId)));
            Assert.True(account.Subscriptions.Any(s => s.Id == new Guid(rdfeSubscription2.SubscriptionId)));
            Assert.True(account.Subscriptions.Any(s => s.Id == new Guid(csmSubscription1.SubscriptionId)));
        }

        [Fact]
        public void GetAzureAccountReturnsAccountWithSubscriptions()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;

            var account = client.GetAzureAccount("test", azureEnvironment.Name).ToList();

            Assert.Equal(1, account.Count);
            Assert.Equal("test", account[0].UserName);
            Assert.Equal(2, account[0].Subscriptions.Count);
            Assert.True(account[0].Subscriptions.Any(s => s.Id == azureSubscription1.Id));
            Assert.True(account[0].Subscriptions.Any(s => s.Id == azureSubscription2.Id));
        }

        [Fact]
        public void GetAzureAccountWithoutEnvironmentReturnsAccount()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;

            var account = client.GetAzureAccount("test", null).ToList();

            Assert.Equal(1, account.Count);
            Assert.Equal("test", account[0].UserName);
            Assert.Equal(2, account[0].Subscriptions.Count);
            Assert.True(account[0].Subscriptions.Any(s => s.Id == azureSubscription1.Id));
            Assert.True(account[0].Subscriptions.Any(s => s.Id == azureSubscription2.Id));
        }

        [Fact]
        public void GetAzureAccountReturnsEmptyEnumerationForNonExistingUser()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;

            var account = client.GetAzureAccount("test2", azureEnvironment.Name).ToList();

            Assert.Equal(0, account.Count);
        }

        [Fact]
        public void GetAzureAccountReturnsAllAccountsWithNullUser()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            azureSubscription3withoutUser.Properties[AzureSubscription.Property.UserAccount] = "test2";
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;

            var account = client.GetAzureAccount(null, azureEnvironment.Name).ToList();

            Assert.Equal(2, account.Count);
        }

        [Fact]
        public void RemoveAzureAccountRemovesSubscriptions()
        {
            SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            azureSubscription3withoutUser.Properties[AzureSubscription.Property.UserAccount] = "test2";
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;
            List<string> log = new List<string>();
            Action<string> logger = log.Add;

            Assert.Equal(3, client.Profile.Subscriptions.Count);

            client.RemoveAzureAccount("test2", logger);

            Assert.Equal(2, client.Profile.Subscriptions.Count);
            Assert.Equal(0, log.Count);
        }

        [Fact]
        public void RemoveAzureAccountRemovesDefaultSubscriptionAndWritesWarning()
        {
           SetMockData();
            MockDataStore dataStore = new MockDataStore();
            ProfileClient.DataStore = dataStore;
            ProfileClient client = new ProfileClient();
            client.Profile.Subscriptions[azureSubscription1.Id] = azureSubscription1;
            client.Profile.Subscriptions[azureSubscription2.Id] = azureSubscription2;
            azureSubscription3withoutUser.Properties[AzureSubscription.Property.UserAccount] = "test2";
            client.Profile.Subscriptions[azureSubscription3withoutUser.Id] = azureSubscription3withoutUser;
            client.Profile.Environments[azureEnvironment.Name] = azureEnvironment;
            PowerShellUtilities.GetCurrentModeOverride = () => AzureModule.AzureResourceManager;
            List<string> log = new List<string>();
            Action<string> logger = log.Add;

            Assert.Equal(3, client.Profile.Subscriptions.Count);

            client.RemoveAzureAccount("test", logger);

            Assert.Equal(1, client.Profile.Subscriptions.Count);
            Assert.Equal(1, log.Count);
            Assert.Equal(
                "The default subscription is being removed. Use Select-AzureSubscription -Default <subscriptionName> to select a new default subscription.",
                log[0]);
        }

        private void SetMocks(List<WindowsAzure.Subscriptions.Models.SubscriptionListOperationResponse.Subscription> rdfeSubscriptions, 
            List<Azure.Subscriptions.Models.Subscription> csmSubscriptions)
        {
            ClientMocks clientMocks = new ClientMocks(defaultSubscription);

            clientMocks.LoadRdfeSubscriptions(rdfeSubscriptions);
            clientMocks.LoadCsmSubscriptions(csmSubscriptions);

            AzureSession.ClientFactory = new MockClientFactory(new object[] { clientMocks.RdfeSubscriptionClientMock.Object,
                clientMocks.CsmSubscriptionClientMock.Object });

            AzureSession.AuthenticationFactory = new MockAuthenticationFactory();
        }

        private void SetMockData()
        {
            rdfeSubscription1 = new Subscriptions.Models.SubscriptionListOperationResponse.Subscription
            {
                SubscriptionId = "16E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E",
                SubscriptionName = "RdfeSub1",
                SubscriptionStatus = Subscriptions.Models.SubscriptionStatus.Active,
                ActiveDirectoryTenantId = "Common"
            };
            rdfeSubscription2 = new Subscriptions.Models.SubscriptionListOperationResponse.Subscription
            {
                SubscriptionId = "26E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E",
                SubscriptionName = "RdfeSub2",
                SubscriptionStatus = Subscriptions.Models.SubscriptionStatus.Active,
                ActiveDirectoryTenantId = "Common"
            };
            csmSubscription1 = new Azure.Subscriptions.Models.Subscription
            {
                Id = "Subscriptions/36E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E",
                DisplayName = "CsmSub1",
                State = "Active",
                SubscriptionId = "36E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E"
            };
            csmSubscription2 = new Azure.Subscriptions.Models.Subscription
            {
                Id = "Subscriptions/46E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E",
                DisplayName = "CsmSub2",
                State = "Active",
                SubscriptionId = "46E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E"
            };
            azureSubscription1 = new AzureSubscription
            {
                Id = new Guid("56E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E"),
                Name = "LocalSub1",
                Environment = "Test",
                Properties = new Dictionary<AzureSubscription.Property,string> { {AzureSubscription.Property.UserAccount, "test"}, {AzureSubscription.Property.Default, "True"}}
            };
            azureSubscription2 = new AzureSubscription
            {
                Id = new Guid("66E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E"),
                Name = "LocalSub2",
                Environment = "Test",
                Properties = new Dictionary<AzureSubscription.Property, string> { { AzureSubscription.Property.UserAccount, "test" } }
            };
            azureSubscription3withoutUser = new AzureSubscription
            {
                Id = new Guid("76E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E"),
                Name = "LocalSub3",
                Environment = "Test",
            };
            azureEnvironment = new AzureEnvironment
            {
                Name = "Test",
                Endpoints = new Dictionary<AzureEnvironment.Endpoint, string>
                {
                    { AzureEnvironment.Endpoint.ServiceEndpoint, "https://umapi.rdfetest.dnsdemo4.com:8443/" },
                    { AzureEnvironment.Endpoint.ManagementPortalUrl, "https://windows.azure-test.net" },
                    { AzureEnvironment.Endpoint.AdTenantUrl, "https://login.windows-ppe.net/" },
                    { AzureEnvironment.Endpoint.ActiveDirectoryEndpoint, "https://login.windows-ppe.net/" },
                    { AzureEnvironment.Endpoint.GalleryEndpoint, "https://current.gallery.azure-test.net" },
                    { AzureEnvironment.Endpoint.ResourceManagerEndpoint, "https://api-current.resources.windows-int.net/" },
                }
            };
            newProfileDataPath = System.IO.Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.ProfileFile);
            oldProfileDataPath = System.IO.Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.OldProfileFile);
            oldProfileData = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <ProfileData xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/Microsoft.WindowsAzure.Commands.Utilities.Common"">
                  <DefaultEnvironmentName>AzureCloud</DefaultEnvironmentName>
                  <Environments>
                    <AzureEnvironmentData>
                      <ActiveDirectoryServiceEndpointResourceId>https://management.core.windows.net/</ActiveDirectoryServiceEndpointResourceId>
                      <AdTenantUrl>https://login.windows-ppe.net/</AdTenantUrl>
                      <CommonTenantId>Common</CommonTenantId>
                      <GalleryEndpoint>https://current.gallery.azure-test.net</GalleryEndpoint>
                      <ManagementPortalUrl>http://go.microsoft.com/fwlink/?LinkId=254433</ManagementPortalUrl>
                      <Name>Current</Name>
                      <PublishSettingsFileUrl>d:\Code\azure.publishsettings</PublishSettingsFileUrl>
                      <ResourceManagerEndpoint>https://api-current.resources.windows-int.net/</ResourceManagerEndpoint>
                      <ServiceEndpoint>https://umapi.rdfetest.dnsdemo4.com:8443/</ServiceEndpoint>
                      <SqlDatabaseDnsSuffix>.database.windows.net</SqlDatabaseDnsSuffix>
                      <StorageEndpointSuffix i:nil=""true"" />
                    </AzureEnvironmentData>
                    <AzureEnvironmentData>
                      <ActiveDirectoryServiceEndpointResourceId>https://management.core.windows.net/</ActiveDirectoryServiceEndpointResourceId>
                      <AdTenantUrl>https://login.windows-ppe.net/</AdTenantUrl>
                      <CommonTenantId>Common</CommonTenantId>
                      <GalleryEndpoint>https://df.gallery.azure-test.net</GalleryEndpoint>
                      <ManagementPortalUrl>https://windows.azure-test.net</ManagementPortalUrl>
                      <Name>Dogfood</Name>
                      <PublishSettingsFileUrl>https://auxnext.windows.azure-test.net/publishsettings/index</PublishSettingsFileUrl>
                      <ResourceManagerEndpoint>https://api-dogfood.resources.windows-int.net</ResourceManagerEndpoint>
                      <ServiceEndpoint>https://management-preview.core.windows-int.net/</ServiceEndpoint>
                      <SqlDatabaseDnsSuffix>.database.windows.net</SqlDatabaseDnsSuffix>
                      <StorageEndpointSuffix i:nil=""true"" />
                    </AzureEnvironmentData>
                  </Environments>
                  <Subscriptions>
                    <AzureSubscriptionData>
                      <ActiveDirectoryEndpoint i:nil=""true"" />
                      <ActiveDirectoryServiceEndpointResourceId i:nil=""true"" />
                      <ActiveDirectoryTenantId i:nil=""true"" />
                      <ActiveDirectoryUserId i:nil=""true"" />
                      <CloudStorageAccount i:nil=""true"" />
                      <GalleryEndpoint i:nil=""true"" />
                      <IsDefault>true</IsDefault>
                      <LoginType i:nil=""true"" />
                      <ManagementCertificate i:nil=""true""/>
                      <ManagementEndpoint>https://management.core.windows.net/</ManagementEndpoint>
                      <Name>Test</Name>
                      <RegisteredResourceProviders xmlns:d4p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" />
                      <ResourceManagerEndpoint i:nil=""true"" />
                      <SqlDatabaseDnsSuffix>.database.windows.net</SqlDatabaseDnsSuffix>
                      <SubscriptionId>06E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1E</SubscriptionId>
                    </AzureSubscriptionData>
	                <AzureSubscriptionData>
                      <ActiveDirectoryEndpoint i:nil=""true"" />
                      <ActiveDirectoryServiceEndpointResourceId i:nil=""true"" />
                      <ActiveDirectoryTenantId i:nil=""true"" />
                      <ActiveDirectoryUserId i:nil=""true"" />
                      <CloudStorageAccount i:nil=""true"" />
                      <GalleryEndpoint i:nil=""true"" />
                      <IsDefault>true</IsDefault>
                      <LoginType i:nil=""true"" />
                      <ManagementCertificate i:nil=""true""/>
                      <ManagementEndpoint>https://management.core.windows.net/</ManagementEndpoint>
                      <Name>Test 2</Name>
                      <RegisteredResourceProviders xmlns:d4p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" />
                      <ResourceManagerEndpoint i:nil=""true"" />
                      <SqlDatabaseDnsSuffix>.database.windows.net</SqlDatabaseDnsSuffix>
                      <SubscriptionId>06E3F6FD-A3AA-439A-8FC4-1F5C41D2AD1F</SubscriptionId>
                    </AzureSubscriptionData>
                  </Subscriptions>
                </ProfileData>";
        }
    }
}
