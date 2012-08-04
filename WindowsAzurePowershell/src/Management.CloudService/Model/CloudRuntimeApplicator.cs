// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Diagnostics;
    using System.Web.Script.Serialization;
    using System.Reflection;

    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;

    public class CloudRuntimeApplicator
    {
        CloudRuntime Runtime { get; set; }
        CloudRuntimePackage Package { get; set;}
        WebRole WebRole { get; set; }
        WorkerRole WorkerRole { get; set; }

        private CloudRuntimeApplicator()
        {
        }

        public static CloudRuntimeApplicator CreateCloudRuntimeApplicator(CloudRuntime cloudRuntime, CloudRuntimePackage cloudRuntimePackage, WebRole role)
        {
            CloudRuntimeApplicator applicator = new CloudRuntimeApplicator
            {
                Runtime = cloudRuntime,
                Package = cloudRuntimePackage,
                WebRole = role
            };

            return applicator;
        }

        public static CloudRuntimeApplicator CreateCloudRuntimeApplicator(CloudRuntime cloudRuntime, CloudRuntimePackage cloudRuntimePackage, WorkerRole role)
        {
            CloudRuntimeApplicator applicator = new CloudRuntimeApplicator 
            {
                Runtime = cloudRuntime, 
                Package = cloudRuntimePackage, 
                WorkerRole = role
            };

            return applicator;
        }

        public void Apply()
        {
            if (this.WorkerRole != null)
            {
                this.Runtime.ApplyRuntime(this.Package, this.WorkerRole);
            }
            else if (this.WebRole != null)
            {
                this.Runtime.ApplyRuntime(this.Package, this.WebRole);
            }
        }
    }
}
