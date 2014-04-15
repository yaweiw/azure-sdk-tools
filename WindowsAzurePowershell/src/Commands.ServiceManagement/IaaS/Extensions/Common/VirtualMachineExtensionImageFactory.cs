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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System.Linq;
    using Management.Compute;
    using Model.PersistentVMModel;

    public class VirtualMachineExtensionImageFactory
    {
        private ComputeManagementClient computeClient;

        public VirtualMachineExtensionImageFactory(ComputeManagementClient computeClient)
        {
            this.computeClient = computeClient;
        }

        public ResourceExtensionReference MakeItem(
            string publisherName,
            string extensionName,
            bool createOnlyIfExists = true)
        {
            ResourceExtensionReference extension = null;

            if (createOnlyIfExists)
            {
                if (this.computeClient != null)
                {
                    var reference = this.computeClient.VirtualMachineExtensions.ListVersions(
                        publisherName,
                        extensionName).FirstOrDefault();

                    if (reference != null)
                    {
                        extension = new ResourceExtensionReference
                        {
                            Publisher     = reference.Publisher,
                            Name          = reference.Name,
                            ReferenceName = reference.Name
                        };
                    }
                }
            }
            else
            {
                extension = new ResourceExtensionReference
                {
                    Publisher     = publisherName,
                    Name          = extensionName,
                    ReferenceName = extensionName
                };
            }

            return extension;
        }

        public ResourceExtensionReferenceList MakeList(
            string publisherName,
            string extensionName,
            bool createOnlyIfExists = true)
        {
            var item = MakeItem(publisherName, extensionName, createOnlyIfExists);
            var list = Enumerable.Repeat<ResourceExtensionReference>(item, 1);
            return item == null ? null : new ResourceExtensionReferenceList(list);
        }
    }
}
