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

namespace Microsoft.WindowsAzure.Management.Subscription
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using Utilities.Common;
    using Utilities.Properties;
    using Utilities.Subscriptions;
    using Utilities.Subscriptions.Contract;

    /// <summary>
    /// Imports publish profiles.
    /// </summary>
    [Cmdlet(VerbsData.Import, "AzurePublishSettingsFile"), OutputType(typeof(string))]
    public class ImportAzurePublishSettingsCommand : CmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the publish settings file.")]
        [ValidateNotNullOrEmpty]
        public string PublishSettingsFile { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the subscription data output file.")]
        public string SubscriptionDataFile { get; set; }

        public ISubscriptionClient SubscriptionClient { get; set; }

        private ISubscriptionClient GetSubscriptionClient(SubscriptionData subscription)
        {
            if (SubscriptionClient == null)
            {
                SubscriptionClient = new SubscriptionClient(subscription);
            }
            return SubscriptionClient;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal SubscriptionData ImportSubscriptionFile(string publishSettingsFile, string subscriptionsDataFile)
        {
            GlobalComponents globalComponents = GlobalComponents.CreateFromPublishSettings(
                GlobalPathInfo.GlobalSettingsDirectory,
                subscriptionsDataFile,
                publishSettingsFile);

            // Set a current and default subscription if possible
            if (globalComponents.Subscriptions != null && globalComponents.Subscriptions.Count > 0)
            {
                var currentDefaultSubscription = globalComponents.Subscriptions.Values.FirstOrDefault(subscription =>
                                                                                                      subscription.IsDefault);
                if (currentDefaultSubscription == null)
                {
                    // Sets the a new default subscription from the imported ones
                    currentDefaultSubscription = globalComponents.Subscriptions.Values.First();
                    currentDefaultSubscription.IsDefault = true;
                }

                if (this.GetCurrentSubscription() == null)
                {
                    this.SetCurrentSubscription(currentDefaultSubscription);
                }

                // Save subscriptions file to make sure publish settings subscriptions get merged
                // into the subscriptions data file and the default subscription is updated.
                globalComponents.SaveSubscriptions(subscriptionsDataFile);

                return currentDefaultSubscription;
            }

            return null;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            string publishSettingsFile = this.TryResolvePath(PublishSettingsFile);
            string subscriptionDataFile = this.TryResolvePath(SubscriptionDataFile);
            string searchDirectory = Directory.Exists(publishSettingsFile) ? publishSettingsFile :
                string.IsNullOrEmpty(publishSettingsFile) ? base.CurrentPath() : string.Empty;
            bool multipleFilesFound = false;
            
            if (!string.IsNullOrEmpty(searchDirectory))
            {
                string[] publishSettingsFiles = Directory.GetFiles(searchDirectory, "*.publishsettings");

                if (publishSettingsFiles.Length > 0)
                {
                    publishSettingsFile = publishSettingsFiles[0];
                    multipleFilesFound = publishSettingsFiles.Length > 1;
                }
                else
                {
                    throw new Exception(string.Format(Resources.NoPublishSettingsFilesFoundMessage, searchDirectory));
                }
            }

            SubscriptionData defaultSubscription = ImportSubscriptionFile(publishSettingsFile, subscriptionDataFile);

            if (defaultSubscription != null)
            {
                WriteVerbose(string.Format(
                    Resources.DefaultAndCurrentSubscription,
                    defaultSubscription.SubscriptionName));
                RegisterResourceProviders(defaultSubscription);
            }

            if (multipleFilesFound)
            {
                WriteWarning(string.Format(Resources.MultiplePublishSettingsFilesFoundMessage, publishSettingsFile));
            }

            if (!string.IsNullOrEmpty(searchDirectory))
            {
                WriteObject(publishSettingsFile);
            }
        }

        private void RegisterResourceProviders(SubscriptionData subscription)
        {
            ISubscriptionClient client = GetSubscriptionClient(subscription);
            var knownProviders = new List<string>(ProviderRegistrationConstants.GetKnownResourceTypes());
            var providers = new List<ProviderResource>(client.ListResources(knownProviders));
            var providersToRegister = GetUnregisteredProviders(knownProviders, 
                providers.Where(p => p.State == ProviderRegistrationConstants.Unregistered)
                .Select(p => p.Type).ToList());

            Task.WaitAll(providersToRegister.Select(client.RegisterResourceTypeAsync).Cast<Task>().ToArray());
        }

        private IEnumerable<string> GetUnregisteredProviders(IEnumerable<string> knownProviders, List<string> registeredProviders)
        {
            return knownProviders.Where(knownProvider => !registeredProviders.Contains(knownProvider));
        }
    }
}
