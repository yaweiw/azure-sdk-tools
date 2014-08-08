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

using System.Collections.Generic;
using Microsoft.WindowsAzure.Commands.Common.Factories;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.IO;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public static class AzureSession
    {
        static AzureSession()
        {
            SubscriptionTokenCache = new Dictionary<Guid, IAccessToken>();
            Environments = new Dictionary<string, AzureEnvironment>();
            ClientFactory = new ClientFactory();
            AuthenticationFactory = new AuthenticationFactory();
        }

        public static void Load(Dictionary<string, AzureEnvironment> envs, AzureSubscription defaultSubscription)
        {
            envs.ForEach(e => Environments[e.Key] = e.Value);

            if (CurrentSubscription == null)
            {
                CurrentSubscription = defaultSubscription;
            }
        }

        public static Dictionary<string, AzureEnvironment> Environments { get; set; }

        public static IDictionary<Guid, IAccessToken> SubscriptionTokenCache { get; set; }

        public static AzureSubscription CurrentSubscription { get; set; }

        public static AzureEnvironment CurrentEnvironment
        {
            get
            {
                if (CurrentSubscription == null)
                {
                    return AzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
                }

                return Environments[CurrentSubscription.Environment];
            }
        }

        public static IClientFactory ClientFactory { get; set; }

        public static IAuthenticationFactory AuthenticationFactory { get; set; }
    }
}
