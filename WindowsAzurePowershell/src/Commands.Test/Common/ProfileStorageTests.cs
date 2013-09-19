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

namespace Microsoft.WindowsAzure.Commands.Test.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using Commands.Utilities.Common;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProfileStorageTests
    {
        private static ProfileData testProfileData = new ProfileData
        {
            DefaultEnvironmentName = "AzureCloud",
            Environments = new[]
                {
                    new AzureEnvironmentData
                    {
                        AdTenantUrl = "adtenant",
                        CommonTenantId = "Common",
                        ManagementPortalUrl = "https://management.windowsazure.net/",
                        ServiceEndpoint = "https://do.some.stuff",
                        Name = "Custom1",
                        PublishSettingsFileUrl = "SomeUrl",
                        StorageBlobEndpointFormat = "blobFormat",
                        StorageQueueEndpointFormat = "queueFormat",
                        StorageTableEndpointFormat = "tableFormat",
                    },
                    new AzureEnvironmentData
                    {
                        AdTenantUrl = "adtenant2",
                        CommonTenantId = "Common",
                        ManagementPortalUrl = "https://management.windowsazure.net/2",
                        ServiceEndpoint = "https://do.some.other.stuff",
                        Name = "Custom2",
                        PublishSettingsFileUrl = "SomeUrl2",
                        StorageBlobEndpointFormat = "blobFormat2",
                        StorageQueueEndpointFormat = "queueFormat2",
                        StorageTableEndpointFormat = "tableFormat2",
                    },

                }
        };

        
        [TestMethod]
        public void DataContractSerializedToXmlAsExpected()
        {
            var profileToSerialize = testProfileData;

            var roundTripped = RoundTrip(profileToSerialize);

            Assert.AreEqual(roundTripped.DefaultEnvironmentName, profileToSerialize.DefaultEnvironmentName);
            Assert.AreEqual(profileToSerialize.Environments.Count(), roundTripped.Environments.Count());
            var environments = roundTripped.Environments.Zip(profileToSerialize.Environments, Tuple.Create);

            foreach (var pair in environments)
            {
                AssertEqual(pair.Item2, pair.Item1);
            }
        }

        [TestMethod]
        public void SavingWritesProfileDataToStore()
        {
            var storeMock = new Mock<IProfileStore>();
            ProfileData savedData = null;
            storeMock.Setup(s => s.Save(It.IsAny<ProfileData>()))
                .Callback((ProfileData data) => { savedData = data; });

            var profile = new WindowsAzureProfile(storeMock.Object);
            var sourceEnv = testProfileData.Environments.First();
            profile.AddEnvironment(new WindowsAzureEnvironment
            {
                ManagementPortalUrl = sourceEnv.ManagementPortalUrl,
                Name = sourceEnv.Name,
                PublishSettingsFileUrl = sourceEnv.PublishSettingsFileUrl,
                ServiceEndpoint = sourceEnv.ServiceEndpoint,
                AdTenantUrl = sourceEnv.AdTenantUrl,
                CommonTenantId = sourceEnv.CommonTenantId,
                StorageBlobEndpointFormat = sourceEnv.StorageBlobEndpointFormat,
                StorageQueueEndpointFormat = sourceEnv.StorageQueueEndpointFormat,
                StorageTableEndpointFormat = sourceEnv.StorageTableEndpointFormat
            });

            storeMock.Verify(s => s.Save(It.IsAny<ProfileData>()), Times.Once);
            Assert.AreEqual(1, savedData.Environments.Count());
            Assert.AreEqual(EnvironmentName.AzureCloud, savedData.DefaultEnvironmentName);
            AssertEqual(sourceEnv, savedData.Environments.First());
        }

        private T RoundTrip<T>(T objectToSerialize)
        {
            var writeSerializer = new DataContractSerializer(typeof (T));
            MemoryStream s = new MemoryStream();
            writeSerializer.WriteObject(s, objectToSerialize);

            s.Seek(0, SeekOrigin.Begin);

            var readSerializer = new DataContractSerializer(typeof (T));
            return (T)readSerializer.ReadObject(s);
        }

        private void AssertEqual(AzureEnvironmentData expected, AzureEnvironmentData actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.AdTenantUrl, actual.AdTenantUrl);
            Assert.AreEqual(expected.CommonTenantId, actual.CommonTenantId);
            Assert.AreEqual(expected.ManagementPortalUrl, actual.ManagementPortalUrl);
            Assert.AreEqual(expected.ServiceEndpoint, actual.ServiceEndpoint);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.PublishSettingsFileUrl, actual.PublishSettingsFileUrl);
            Assert.AreEqual(expected.StorageBlobEndpointFormat, actual.StorageBlobEndpointFormat);
            Assert.AreEqual(expected.StorageQueueEndpointFormat, actual.StorageQueueEndpointFormat);
            Assert.AreEqual(expected.StorageTableEndpointFormat, actual.StorageTableEndpointFormat);
        }
    }
}
