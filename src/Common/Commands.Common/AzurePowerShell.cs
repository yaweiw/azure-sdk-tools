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

using Microsoft.WindowsAzure.Commands.Common.Factories;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public class AzurePowerShell
    {
        private static string defaultProfilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Resources.AzureDirectoryName);

        private static AzureSubscription currentSubscription;

        // TODO: Token Cache static property

        static AzurePowerShell()
        {
            ClientFactoryInitializer = (p, a) => { return new ClientFactory(p, a); };
            AuthenticationFactoryInitializer = p => { return new AuthenticationFactory(p); };
        }

        public const string AssemblyCompany = "Microsoft";

        public const string AssemblyProduct = "Microsoft Azure Powershell";

        public const string AssemblyCopyright = "Copyright © Microsoft";

        public const string AssemblyVersion = "0.8.6";

        public const string AssemblyFileVersion = "0.8.6";

        public AzurePowerShell(string profilePath)
        {
            Profile = new AzureProfile(new DiskDataStore(profilePath));
            AuthenticationFactory = AuthenticationFactoryInitializer(Profile);
            ClientFactory = ClientFactoryInitializer(Profile, AuthenticationFactory);
        }

        public AzurePowerShell() : this(defaultProfilePath)
        {

        }

        public static Func<AzureProfile, IAuthenticationFactory, IClientFactory> ClientFactoryInitializer { get; set; }

        public static Func<AzureProfile, IAuthenticationFactory> AuthenticationFactoryInitializer { get; set; }

        public AzureSubscription CurrentSubscription
        {
            get
            {
                return currentSubscription ?? Profile.DefaultSubscription;
            }

            set
            {
                currentSubscription = value;
            }
        }

        public AzureEnvironment CurrentEnvironment
        {
            get
            {
                string env = CurrentSubscription == null ? EnvironmentName.AzureCloud : CurrentSubscription.Environment;
                return Profile.Environments[env];
            }
        }

        public AzureProfile Profile { get; set; }

        public IClientFactory ClientFactory { get; set; }

        public IAuthenticationFactory AuthenticationFactory { get; set; }
    }
}
