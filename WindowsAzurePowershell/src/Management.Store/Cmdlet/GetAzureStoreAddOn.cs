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
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.Contract;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.ResourceModel;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet.Common;
    using System.Globalization;

    /// <summary>
    /// Gets all purchased Add-Ons or specific Add-On
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStoreAddOn"), OutputType(typeof(List<PSObject>), typeof(PSObject))]
    public class GetAzureStoreAddOnCommand : CloudBaseCmdlet<IStoreManagement>
    {
        const string StoreServicePrefix = "Azure-Stores";

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On provider")]
        public string Provider { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Add-On location")]
        public string Location { get; set; }

        /// <summary>
        /// Comapres two strings with handling special case that base string can be empty.
        /// </summary>
        /// <param name="leftHandSide">The base string.</param>
        /// <param name="rightHandSide">The comparer string.</param>
        /// <returns>True if equals or leftHandSide is null/empty, false otherwise.</returns>
        private bool TryEquals(string leftHandSide, string rightHandSide)
        {
            if (string.IsNullOrEmpty(leftHandSide) || leftHandSide.Equals(rightHandSide))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts an AddOn object into PSObject
        /// </summary>
        /// <param name="location">The add on location</param>
        /// <param name="addOn">The add on object</param>
        /// <returns>The PSObject</returns>
        private PSObject AddOnToPSObject(string location, Resource addOn)
        {
            PSObject psObject = ConstructPSObject(typeof(Resource).FullName,
                Parameter.Name, addOn.Name,
                Parameter.Provider, addOn.ResourceProviderNamespace,
                Parameter.AddOn, addOn.Type,
                Parameter.Plan, addOn.Plan,
                Parameter.Location, location,
                Parameter.SchemaVersion, addOn.SchemaVersion,
                Parameter.State, addOn.State,
                Parameter.OperationStatus, addOn.OperationStatus,
                Parameter.OutputItems, addOn.OutputItems ?? new OutputItemList(),
                Parameter.UsageMeters, addOn.UsageMeters ?? new UsageMeterList());

            return psObject;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            CloudServiceList cloudServices = Channel.ListCloudServices(CurrentSubscription.SubscriptionId);
            List<CloudService> storeServices = cloudServices.FindAll(c => CultureInfo.CurrentCulture.CompareInfo.IsPrefix(c.Name, StoreServicePrefix));
            List<PSObject> outputObject = new List<PSObject>();

            foreach (CloudService storeService in storeServices)
            {
                if (TryEquals(Location, storeService.GeoRegion))
                {
                    foreach (Resource addOn in storeService.Resources)
                    {
                        if (TryEquals(Name, addOn.Name) && TryEquals(Provider, addOn.ResourceProviderNamespace))
                        {
                            outputObject.Add(AddOnToPSObject(storeService.GeoRegion, addOn));
                        }
                    }
                }
            }

            if (outputObject.Count.Equals(1))
            {
                WriteObject(outputObject[0]);
            }
            else
            {
                WriteObject(outputObject);
            }
        }
    }
}