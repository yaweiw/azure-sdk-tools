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

namespace Microsoft.WindowsAzure.Commands.ManagedCache
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ManagedCache;

    /// <summary>
    /// Retrieves a list of Windows Azure SQL Database servers in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureManagedCache", ConfirmImpact = ConfirmImpact.None)]
    public class SetAzureManagedCache : ManagedCacheCmdletBase
    {
        [Parameter(Position = 0, 
            HelpMessage = "azure cache service name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 1, 
            Mandatory = true, 
            HelpMessage = "The geographic region to create the website.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Position = 2,
            Mandatory = false)]
        [ValidateSet("Basic", "Standard", "Premium", IgnoreCase = true)]
        public string Sku
        {
            get;
            set;
        }

        [Parameter(Position = 3,
            Mandatory = false,
            HelpMessage = "The cache memeory size")]
        public string Memory
        {
            get;
            set;
        }

        [Parameter(Mandatory = false)]
        [Alias("f")]
        public SwitchParameter ForceUpdate { get; set; }

        public override void ExecuteCmdlet()
        {
            throw new NotImplementedException("NYI");
        }      
    }
}