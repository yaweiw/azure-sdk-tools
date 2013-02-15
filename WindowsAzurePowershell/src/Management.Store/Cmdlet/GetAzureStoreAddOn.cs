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
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.ServiceManagement.Store.Contract;
    using Microsoft.WindowsAzure.ServiceManagement.Store.ResourceModel;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Store.Model;

    /// <summary>
    /// Gets all purchased Add-Ons or specific Add-On
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStoreAddOn"), OutputType(typeof(List<AddOn>), typeof(AddOn))]
    public class GetAzureStoreAddOnCommand : CloudBaseCmdlet<IStoreManagement>
    {
        public StoreClient StoreClient { get; set; }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On provider")]
        public string Provider { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On location")]
        public string Location { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            StoreClient = StoreClient ?? new StoreClient(
                CurrentSubscription.SubscriptionId,
                ServiceEndpoint,
                CurrentSubscription.Certificate,
                text => this.WriteDebug(text));
            List<AddOn> addOns = StoreClient.GetAddOn(new AddOnSearchOptions(Name, Provider, Location));

            if (addOns.Count == 1)
            {
                WriteObject(addOns);
            }
            else
            {
                WriteObject(addOns, true);
            }
        }
    }
}