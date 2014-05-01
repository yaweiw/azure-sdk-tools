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

namespace Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models
{
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;

    public class SimpleProfile
    {
        private Profile profile { get; set; }

        public SimpleProfile(Profile profile)
        {
            this.profile = profile;
        }

        public string Name
        {
            get { return profile.Name; }
            set { profile.Name = value; }
        }

        public string DomainName
        {
            get { return profile.DomainName; }
            set { profile.DomainName = value; }
        }

        public ProfileDefinitionStatus Status
        {
            get { return profile.Status; }
            set { profile.Status = value; }
        }
    }
}
