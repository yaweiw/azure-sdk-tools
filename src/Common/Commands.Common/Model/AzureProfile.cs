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

using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Model
{
    public class AzureProfile
    {
        private static AzureProfile instance = null;

        private AzureSubscription current;

        private AzureProfile ()
        {
            if (Store == null)
            {
                // Create the default store which reads from the file
            }
            else
            {
                // Use the specified store to fill out Environments and Subscriptions.
            }
        }

        public static IProfileStore Store { private get; set; }

        public static AzureProfile Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureProfile();
                }

                return instance;
            }
        }

        public AzureSubscription CurrentSubscription
        {
            get
            {
                if (current != null)
                {
                    return current;
                }
                else
                {
                    return Subscriptions.Get(CurrentEnvironment.DefaultSubscription);
                }
            }

            set
            {
                current = value;
            }
        }

        public AzureEnvironment CurrentEnvironment
        {
            get { return Environments.Get(EnvironmentName.AzureCloud); }
        }

        public AzureEnvironments Environments { get; set; }

        public AzureSubscriptions Subscriptions { get; set; }

        public Uri GetEndpoint(AzureSubscription subscription, AzureEnvironment.Endpoint endpoint)
        {
            return new Uri(Environments.GetEndpoint(subscription.Environment, endpoint));
        }
    }
}
