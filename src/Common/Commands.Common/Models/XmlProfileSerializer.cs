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

using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public class XmlProfileSerializer : IProfileSerializer
    {
        public string Serialize(AzureProfile obj)
        {
            throw new NotImplementedException();
        }

        public AzureProfile Deserialize(string contents)
        {
            ProfileData data = null;
            AzureProfile profile = new AzureProfile(new VirtualDiskDataStore());

            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ProfileData));
                using (MemoryStream s = new MemoryStream(Encoding.UTF8.GetBytes(contents ?? "")))
                {
                    data = (ProfileData)serializer.ReadObject(s);
                }
            }
            catch (XmlException) { }

            if (data != null)
            {
                foreach (AzureEnvironmentData oldEnv in data.Environments)
                {
                    profile.AddEnvironment(oldEnv.ToAzureEnvironment());
                }

                List<AzureEnvironment> envs = profile.Environments;
                foreach (AzureSubscriptionData oldSubscription in data.Subscriptions)
                {
                    profile.AddSubscription(oldSubscription.ToAzureSubscription(envs));

                    if (!string.IsNullOrEmpty(oldSubscription.ManagementCertificate))
                    {
                        profile.AddCertificate(GeneralUtilities.GetCertificateFromStore(oldSubscription.ManagementCertificate));
                    }
                }
            }

            return profile;
        }

        public string ProfileFile { get { return "WindowsAzureProfile.xml"; } }
    }
}
