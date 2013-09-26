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

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus.Contract;
    using Commands.Utilities.ServiceBus.ResourceModel;

    /// <summary>
    /// Lists all service bus locations available for a subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSBLocation"), OutputType(typeof(List<ServiceBusRegion>))]
    public class GetAzureSBLocationCommand : CloudBaseCmdlet<IServiceBusManagement>
    {
        /// <summary>
        /// Initializes a new instance of the GetAzureSBLocationCommand class.
        /// </summary>
        public GetAzureSBLocationCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureSBLocationCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureSBLocationCommand(IServiceBusManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Gets list of service bus regions on the given subscription.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            List<ServiceBusRegion> regions = Channel.ListServiceBusRegions(CurrentSubscription.SubscriptionId);
            WriteObject(regions, true);
        }
    }
}