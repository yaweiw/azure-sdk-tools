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
            CurrentEnvironment = AzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
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

        public static AzureSubscription CurrentSubscription { get; private set; }

        public static AzureEnvironment CurrentEnvironment {get; private set; }
        
        public static void SetCurrentSubscription(AzureSubscription subscription, AzureEnvironment environment)
        {
            if (subscription == null)
            {
                CurrentSubscription = null;
            }

            if (environment == null)
            {
                CurrentEnvironment = AzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
            }

            if (subscription != null && environment != null)
            {
                if (subscription.Environment != environment.Name)
                {
                    throw new ArgumentException("Environment name doesn't match one in subscription.", "environment");
                }

                CurrentSubscription = subscription;
                CurrentEnvironment = environment;
            }
        }

        public static IClientFactory ClientFactory { get; set; }

        public static IAuthenticationFactory AuthenticationFactory { get; set; }
    }
}
