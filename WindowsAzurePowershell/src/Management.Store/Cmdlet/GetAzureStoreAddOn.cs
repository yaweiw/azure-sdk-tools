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
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.Contract;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.ResourceModel;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet.Common;
    using Microsoft.WindowsAzure.Management.Store.Model;

    /// <summary>
    /// Gets all purchased Add-Ons or specific Add-On
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStoreAddOn"), OutputType(typeof(List<PSObject>), typeof(PSObject))]
    public class GetAzureStoreAddOnCommand : CloudBaseCmdlet<IStoreManagement>
    {
        public StoreClient StoreClient { get; set; }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On provider")]
        public string Provider { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On location")]
        public string Location { get; set; }

        /// <summary>
        /// Converts an AddOn object into PSObject
        /// </summary>
        /// <param name="location">The add on location</param>
        /// <param name="addOn">The add on object</param>
        /// <returns>The PSObject</returns>
        internal PSObject AddOnToPSObject(AddOn addOn)
        {
            PSObject psObject = ConstructPSObject(typeof(Resource).FullName,
                Parameter.Name, addOn.Info.Name,
                Parameter.Provider, addOn.Info.ResourceProviderNamespace,
                Parameter.AddOn, addOn.Info.Type,
                Parameter.Plan, addOn.Info.Plan,
                Parameter.Location, addOn.GeoRegion,
                Parameter.SchemaVersion, addOn.Info.SchemaVersion,
                Parameter.State, addOn.Info.State,
                Parameter.LastOperationStatus, addOn.Info.OperationStatus,
                Parameter.OutputItems, addOn.Info.OutputItems ?? new OutputItemList(),
                Parameter.UsageMeters, addOn.Info.UsageMeters ?? new UsageMeterList());

            return psObject;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            StoreClient = StoreClient ?? new StoreClient(
                CurrentSubscription.SubscriptionId,
                ServiceEndpoint,
                CurrentSubscription.Certificate,
                text => this.WriteDebug(text));
            List<AddOn> addOns = StoreClient.GetAddOn(new AddOnSearchOptions(Name, Provider, Location));
            List<PSObject> outputObject = new List<PSObject>();
            addOns.ForEach(addOn => outputObject.Add(AddOnToPSObject(addOn)));

            if (outputObject.Count.Equals(1))
            {
                WriteObject(outputObject[0]);
            }
            else
            {
                WriteObject(outputObject, true);
            }
        }
    }
}