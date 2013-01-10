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

namespace Microsoft.WindowsAzure.Management.Storage.Blob
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract;
    using System.Security.Permissions;

    [Cmdlet(VerbsCommon.Remove, "AzureStorageBlob", DefaultParameterSetName = "ContainerBlobNameManual")]
    public class RemoveStorageAzureBlobCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(HelpMessage = "ICloudBlob Object",
                   ValueFromPipeline = true, ParameterSetName = "BlobPipeline")]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "CloudBlobContainer Object",
                  ValueFromPipeline = true, ParameterSetName = "ContainerPipeline")]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(ParameterSetName = "ContainerPipeline", Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        [Parameter(ParameterSetName = "ContainerBlobNameManual", Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        public string Blob 
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        private string BlobName = String.Empty;

        [Parameter(HelpMessage = "Container name", Mandatory = true, Position = 1,
            ParameterSetName = "ContainerBlobNameManual")]
        [ValidateNotNullOrEmpty]
        public string Container
        {
            get { return ContainerName; }
            set { ContainerName = value; }
        }
        private string ContainerName = String.Empty;

        /// <summary>
        /// Initializes a new instance of the RemoveStorageAzureBlobCommand class.
        /// </summary>
        public RemoveStorageAzureBlobCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveStorageAzureBlobCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public RemoveStorageAzureBlobCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        internal void RemoveAzureBlobByICloudBlob(ICloudBlob blob, bool isValidBlob = false)
        {
            if (!isValidBlob)
            {
                ValidatePipelineICloudBlob(blob);
            }
            DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None;
            AccessCondition accessCondition = null;
            BlobRequestOptions requestOptions = null;
            Channel.DeleteICloudBlob(blob, deleteSnapshotsOption, accessCondition, requestOptions, OperationContext);
            string result = String.Format(Resources.RemoveBlobSuccessfully, blob.Name, blob.Container.Name);
            WriteObject(result);
        }

        internal void RemoveAzureBlobByCloudBlobContainer(CloudBlobContainer container, string blobName)
        {
            if (!NameUtil.IsValidBlobName(blobName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidBlobName, blobName));
            }

            ValidatePipelineCloudBlobContainer(container);
            AccessCondition accessCondition = null;
            BlobRequestOptions requestOptions = null;
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, requestOptions, OperationContext);
            if (null == blob)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, container.Name));
            }
            RemoveAzureBlobByICloudBlob(blob, true);
        }

        internal void RemoveAzureBlobByName(string containerName, string blobName)
        {
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            RemoveAzureBlobByCloudBlobContainer(container, blobName);
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName.ToLower())
            {
                case "BlobPipeline":
                    RemoveAzureBlobByICloudBlob(ICloudBlob, false);
                    break;
                case "ContainerPipeline":
                    RemoveAzureBlobByCloudBlobContainer(CloudBlobContainer, ContainerName);
                    break;
                case "ContainerBlobNameManual":
                default:
                    RemoveAzureBlobByName(ContainerName, BlobName);
                    break;
            }
        }
    }
}