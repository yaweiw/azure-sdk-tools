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

namespace Microsoft.Azure.Commands.ManagedCache
{
    using System;
    using System.Management.Automation;
    using Microsoft.Azure.Commands.ManagedCache.Models;

    /// <summary>
    /// Retrieves a list of Windows Azure SQL Database servers in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureManagedCache"), OutputType(typeof(PSCacheService))]
    public class SetAzureManagedCache : ManagedCacheCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Location { get; set;}

        [ValidateSet("Basic", "Standard", "Premium", IgnoreCase = true)]
        public string Sku { get; set; }

        public string Memory { get; set; }

        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            throw new NotImplementedException("NYI");
        }      
    }
}