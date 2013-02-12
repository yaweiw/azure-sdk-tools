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

namespace Microsoft.WindowsAzure.Management.Store.Model
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;
    using System.Collections.Generic;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.ResourceModel;

    public class AddOn
    {
        public Resource Info { get; set; }

        public string GeoRegion { get; set; }

        /// <summary>
        /// Creates new instance from AddOn
        /// </summary>
        /// <param name="info">The add on details</param>
        /// <param name="geoRegion">The add on region</param>
        public AddOn(Resource info, string geoRegion)
        {
            Info = info;

            GeoRegion = geoRegion;
        }
    }
}
