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
using System.Linq;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Utilities.Profile
{
    using Common;
    using Azure.Subscriptions;
    using WindowsAzure.Commands.Common.Properties;
    using System;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Base class for cmdlets that manipulate the
    /// azure subscription, provides common support
    /// for the SubscriptionDataFile parameter.
    /// </summary>
    public abstract class SubscriptionCmdletBase : CmdletWithSubscriptionBase
    {
        [Parameter(Mandatory = false, HelpMessage = "File storing subscription data, it not set uses default.")]
        public string SubscriptionDataFile { get; set; }

        private WindowsAzureProfile loadedProfile;

        private readonly bool createFileIfNotExists;

        protected SubscriptionCmdletBase(bool createFileIfNotExists)
        {
            this.createFileIfNotExists = createFileIfNotExists;
        }

        public override WindowsAzureProfile Profile
        {
            get
            {
                if (loadedProfile == null && !string.IsNullOrEmpty(SubscriptionDataFile))
                {
                    loadedProfile = LoadSubscriptionDataFile();
                }
                if (loadedProfile != null)
                {
                    return loadedProfile;
                }
                return base.Profile;

            }
            set
            {
                base.Profile = value;
            }
        }

        public WindowsAzureProfile BaseProfile
        {
            get { return base.Profile; }
        }

        private WindowsAzureProfile LoadSubscriptionDataFile()
        {
            string path = GetUnresolvedProviderPathFromPSPath(SubscriptionDataFile);
            if (!File.Exists(path) && !createFileIfNotExists)
            {
                throw new Exception(string.Format(Resources.SubscriptionDataFileNotFound, SubscriptionDataFile));
            }
            return new WindowsAzureProfile(new PowershellProfileStore(path));
        }

        protected IEnumerable<AzureSubscription> LoadSubscriptionsFromServer()
        {
            var currentMode = PowerShellUtilities.GetCurrentMode();
            var currentSubscription = AzurePowerShell.Profile.CurrentSubscription;
            var currentEnvironment = AzurePowerShell.Profile.CurrentEnvironment;
            string userId = null;
            if (currentSubscription == null)
            {
                return AzurePowerShell.AuthenticationFactory.Authenticate(currentEnvironment, currentMode, true, out userId);

                // Save userId into environment
                // Save subscriptions into profile
                // Save profile
            }
            else
            {
                // Get all AD accounts and iterate

                userId = currentSubscription.GetProperty(AzureSubscription.Property.UserAccount);
                if (userId != null)
                {
                    return AzurePowerShell.AuthenticationFactory.Authenticate(currentEnvironment, currentMode, true, userId);

                    // Save userId into environment
                    // Save subscriptions into profile
                    // Save profile
                }
                else
                {
                    return new AzureSubscription[0];
                }
            }
        }
    }
}