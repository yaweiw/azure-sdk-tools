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

    [Cmdlet(VerbsCommon.Remove, StorageNouns.Blob, DefaultParameterSetName = NameParameterSet),
        OutputType(typeof(AzureStorageBlob))]
    public class RemoveStorageAzureBlobCommand : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// Blob Pipeline parameter set name
        /// </summary>
        private const string BlobPipelineParameterSet = "BlobPipeline";

        /// <summary>
        /// container pipeline paremeter set name
        /// </summary>
        private const string ContainerPipelineParmeterSet = "ContainerPipeline";

        /// <summary>
        /// blob name and container name parameter set
        /// </summary>
        private const string NameParameterSet = "NamePipeline";

        [Parameter(HelpMessage = "ICloudBlob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = BlobPipelineParameterSet)]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "CloudBlobContainer Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(ParameterSetName = ContainerPipelineParmeterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        [Parameter(ParameterSetName = NameParameterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        public string Blob 
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        private string BlobName = String.Empty;

        [Parameter(HelpMessage = "Container name", Mandatory = true, Position = 1,
            ParameterSetName = NameParameterSet)]
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

        /// <summary>
        /// remove the azure blob 
        /// </summary>
        /// <param name="blob">ICloudblob object</param>
        /// <param name="isValidBlob">whether the ICloudblob parameter is validated</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal void RemoveAzureBlob(ICloudBlob blob, bool isValidBlob = false)
        {
            if (!isValidBlob)
            {
                ValidatePipelineICloudBlob(blob);
            }

            DeleteSnapshotsOption deleteSnapshotsOption = DeleteSnapshotsOption.None;
            AccessCondition accessCondition = null;
            BlobRequestOptions requestOptions = null;

            Channel.DeleteICloudBlob(blob, deleteSnapshotsOption, accessCondition, requestOptions, OperationContext);
        }

        /// <summary>
        /// remove azure blob
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">blob name</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal void RemoveAzureBlob(CloudBlobContainer container, string blobName)
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

            RemoveAzureBlob(blob, true);
        }

        /// <summary>
        /// remove azure blob
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <param name="blobName">blob name</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal void RemoveAzureBlob(string containerName, string blobName)
        {
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            RemoveAzureBlob(container, blobName);
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            string blobName = string.Empty;
            string containerName = string.Empty;

            switch (ParameterSetName)
            {
                case BlobPipelineParameterSet:
                    RemoveAzureBlob(ICloudBlob, false);
                    blobName = ICloudBlob.Name;
                    containerName = ICloudBlob.Container.Name;
                    break;

                case ContainerPipelineParmeterSet:
                    RemoveAzureBlob(CloudBlobContainer, BlobName);
                    blobName = BlobName;
                    containerName = ContainerName;
                    break;

                case NameParameterSet:
                default:
                    RemoveAzureBlob(ContainerName, BlobName);
                    blobName = BlobName;
                    containerName = ContainerName;
                    break;
            }

            string result = String.Format(Resources.RemoveBlobSuccessfully, blobName, containerName);
            WriteObject(result);
        }
    }
}