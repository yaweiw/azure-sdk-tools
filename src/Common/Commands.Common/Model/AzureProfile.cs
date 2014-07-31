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
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.Common.Model
{
    public partial class AzureProfile
    {
        private Guid? currentSubscriptionId;

        public AzureProfile(IFileStore store)
        {
            Load(store);

            foreach (AzureEnvironment env in Environments)
            {
                if (env.DefaultSubscriptionId.HasValue)
                {
                    currentSubscriptionId = env.DefaultSubscriptionId.Value;
                    break;
                }
            }
        }

        public AzureProfile() : this(new DiskFileStore())
        {
            
        }

        public Guid? CurrentSubscriptionId
        {
            get { return currentSubscriptionId; }

            set
            {
                if (GetSubscription(currentSubscriptionId) != null)
                {
                    currentSubscriptionId = value;
                }
            }
        }

        public AzureEnvironment CurrentEnvironment
        {
            get
            {
                if (CurrentSubscriptionId == null)
                {
                    return GetEnvironment(EnvironmentName.AzureCloud);
                }
                else
                {
                    return GetEnvironment(GetSubscription(CurrentSubscriptionId).Environment);
                }
            }
        }

        public List<AzureEnvironment> Environments { get; set; }

        public List<AzureSubscription> Subscriptions { get; set; }
    }
}
