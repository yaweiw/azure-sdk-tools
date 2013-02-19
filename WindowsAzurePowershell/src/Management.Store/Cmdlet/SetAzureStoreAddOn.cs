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

namespace Microsoft.WindowsAzure.Management.Store.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Microsoft.WindowsAzure.Management.Store.Properties;

    /// <summary>
    /// Purchase a new Add-On from Windows Azure Store.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureStoreAddOn"), OutputType(typeof(bool))]
    public class SetAzureStoreAddOnCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public StoreClient StoreClient { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On plan id")]
        public string Plan { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On promotion code")]
        public string PromotionCode { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            StoreClient = StoreClient ?? new StoreClient(
                CurrentSubscription.SubscriptionId,
                ServiceEndpoint,
                CurrentSubscription.Certificate,
                text => this.WriteDebug(text),
                Channel);

            List<WindowsAzureAddOn> addons = StoreClient.GetAddOn(new AddOnSearchOptions(Name));
            if (addons.Count == 1)
            {
                string message = StoreClient.GetConfirmationMessage(OperationType.Set, addons[0].AddOn, Plan);
                bool purchase = Utilities.ShouldProcess(Host, Resources.SetAddOnConformation, message);

                if (purchase)
                {
                    StoreClient.UpdateAddOn(Name, Plan, PromotionCode);
                    WriteVerbose(string.Format(Resources.AddOnUpdatedMessage, Name));
                    WriteObject(true);
                }
            }
            else if(addons.Count == 0)
            {
                throw new Exception(string.Format(Resources.AddOnNotFound, Name));
            }
            else
            {
                throw new Exception(string.Format(Resources.MultipleAddOnsFoundMessage, Name));
            }
        }
    }
}