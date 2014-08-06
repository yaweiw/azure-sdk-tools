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
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Convenience client for azure subscriptions.
    /// </summary>
    public class AzureSubscriptionsClient
    {
        private AzureProfile profile;

        public AzureSubscriptionsClient() : this(AzurePowerShell.Profile)
        { }

        public AzureSubscriptionsClient(AzureProfile profile)
        {
            this.profile = profile;
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromPublishSettingsFile(string filePath)
        {
            var currentEnvironment = profile.CurrentEnvironment;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("File path is not valid.", "filePath");
            }
            return PublishSettingsImporter.ImportAzureSubscription(File.OpenRead(filePath), currentEnvironment.Name);
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromServer()
        {
            var currentMode = PowerShellUtilities.GetCurrentMode();
            var currentSubscription = profile.CurrentSubscription;
            var currentEnvironment = profile.CurrentEnvironment;
            if (currentSubscription == null)
            {
                string userId = null;
                return AzurePowerShell.AuthenticationFactory.Authenticate(currentEnvironment, currentMode, true, out userId);
            }
            else
            {
                // Get all AD accounts and iterate
                var userIds = profile.Subscriptions
                    .Select(s => s.Properties[AzureSubscription.Property.UserAccount]).Distinct();

                List<AzureSubscription> subscriptions = new List<AzureSubscription>();
                foreach (var userId in userIds)
                {
                    subscriptions = subscriptions.Union(AzurePowerShell.AuthenticationFactory
                        .Authenticate(currentEnvironment, currentMode, true, userId)).ToList();
                }

                if (subscriptions.Any())
                {
                    return subscriptions;
                }
                else
                {
                    return new AzureSubscription[0];
                }
            }
        }

        public void SaveSubscriptions(IEnumerable<AzureSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                profile.AddSubscription(subscription);
            }
        }
    }
}