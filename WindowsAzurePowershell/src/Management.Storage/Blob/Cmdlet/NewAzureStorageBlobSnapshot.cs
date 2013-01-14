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

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;

    [Cmdlet(VerbsCommon.New, StorageNouns.BlobSnapshot),
        OutputType(typeof(AzureStorageBlob))]
    public class NewAzureStorageBlobSnapshotCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(HelpMessage = "Azure Blob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNull]
        public ICloudBlob ICloudBlob { get; set; }

        /// <summary>
        /// Initializes a new instance of the NewAzureStorageBlobSnapshotCommand class.
        /// </summary>
        public NewAzureStorageBlobSnapshotCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureStorageBlobSnapshotCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public NewAzureStorageBlobSnapshotCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// create a snapshot for specified blob
        /// </summary>
        /// <param name="blob">the source ICloudBlobObject</param>
        /// <returns></returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob createBlobSnapshot(ICloudBlob blob)
        {
            ValidatePipelineICloudBlob(blob);

            IDictionary<string, string> metadata = null;
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;

            ICloudBlob cloudBlob = Channel.CreateSnapshot(blob, metadata, accessCondition, options, OperationContext);

            if (cloudBlob == null)
            {
                throw new ArgumentException(String.Format(Resources.InvalidBlobType, blob.Name));
            }

            return new AzureStorageBlob(cloudBlob); ;
        }


        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureStorageBlob blob = createBlobSnapshot(ICloudBlob);
            WriteObjectWithStorageContext(blob);
        }
    }
}
