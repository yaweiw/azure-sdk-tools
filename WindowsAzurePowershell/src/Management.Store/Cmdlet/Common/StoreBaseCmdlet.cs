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

namespace Microsoft.WindowsAzure.Management.Store.Cmdlet.Common
{
    using System;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using WindowsAzure.ServiceManagement;
    using WindowsAzure.ServiceManagement.Marketplace.Contract;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.Store.Properties;
    using Microsoft.WindowsAzure.Management.Utilities;

    public class StoreBaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        public IMarketplaceManagement MarketplaceChannel { get; set; }

        public StoreBaseCmdlet()
        {
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            Validate.ValidateInternetConnection();
            InitChannelCurrentSubscription();

            MarketplaceChannel = ServiceManagementHelper.CreateServiceManagementChannel<IMarketplaceManagement>(
                ServiceBinding,
                new Uri(Resources.MarketplaceEndpoint),
                CurrentSubscription.Certificate,
                new HttpRestMessageInspector(this));
            
            ExecuteCmdlet();
        }
    }
}
