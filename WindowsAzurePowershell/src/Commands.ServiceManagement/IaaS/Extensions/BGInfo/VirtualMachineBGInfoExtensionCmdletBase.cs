﻿// ----------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using System.Linq;
    using Management.Compute;
    using Model.PersistentVMModel;

    public class VirtualMachineBGInfoExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
        protected const string VirtualMachineBGInfoExtensionNoun = "AzureVMBGInfoExtension";

        public const string ExtensionDefaultPublisher = "Microsoft.Compute";
        public const string ExtensionDefaultName = "BGInfo";
        protected const string LegacyReferenceName = "MyBGInfoExtension";

        public VirtualMachineBGInfoExtensionCmdletBase()
        {
            base.publisherName = ExtensionDefaultPublisher;
            base.extensionName = ExtensionDefaultName;
        }

        public static ResourceExtensionReferenceList GetSingleExtensionList(ComputeManagementClient computeClient)
        {
            var extensionList = computeClient.VirtualMachineExtensions.ListVersions(
                VirtualMachineBGInfoExtensionCmdletBase.ExtensionDefaultPublisher,
                VirtualMachineBGInfoExtensionCmdletBase.ExtensionDefaultName);

            var referenceList = new ResourceExtensionReferenceList();

            if (extensionList.Any())
            {
                var extensionImage = extensionList.FirstOrDefault();
                var defaultRefName = extensionImage.Name;

                referenceList.Add(new ResourceExtensionReference
                {
                    Publisher     = extensionImage.Publisher,
                    Name          = extensionImage.Name,
                    ReferenceName = defaultRefName
                });
            }

            return referenceList;
        }
    }
}
