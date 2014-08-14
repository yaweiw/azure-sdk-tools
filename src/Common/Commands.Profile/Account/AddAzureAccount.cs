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

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using System;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Commands.Utilities.Profile;

namespace Microsoft.WindowsAzure.Commands.Profile
{
    /// <summary>
    /// Cmdlet to log into an environment and download the subscriptions
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureAccount")]
    [OutputType(typeof(AzureAccount))]
    public class AddAzureAccount : SubscriptionCmdletBase
    {
        [Parameter(Mandatory = false, HelpMessage = "Environment containing the account to log into")]
        public string Environment { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Optional credential")]
        public PSCredential Credential { get; set; }

        public AddAzureAccount() : base(true)
        {
        }

        public override void ExecuteCmdlet()
        {
            string environment = ChosenEnvironment();
            UserCredentials userCredentials = new UserCredentials();
            if (Credential != null)
            {
                userCredentials.UserName = Credential.UserName;
                userCredentials.Password = Credential.Password;
                userCredentials.NoPrompt = false;
            }

            var account = ProfileClient.AddAzureAccount(userCredentials, environment);

            WriteVerbose(string.Format(Resources.AddAccountAdded, userCredentials.UserName));
            WriteVerbose(string.Format(Resources.AddAccountShowDefaultSubscription, Profile.DefaultSubscription.SubscriptionName));
            WriteVerbose(Resources.AddAccountViewSubscriptions);
            WriteVerbose(Resources.AddAccountChangeSubscription);
            WriteObject(account);
        }

        private string ChosenEnvironment()
        {
            if (string.IsNullOrEmpty(Environment))
            {
                return AzureSession.CurrentEnvironment.Name;
            }

            if (!ProfileClient.Profile.Environments.ContainsKey(Environment))
            {
                throw new Exception(string.Format(Resources.EnvironmentNotFound, Environment));
            }
            return Environment;
        }

    }
}