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

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model.Contract;
    using Microsoft.WindowsAzure.Management.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;

    [Cmdlet(VerbsLifecycle.Start, StorageNouns.CopyBlob, DefaultParameterSetName = NameParameterSet),
       OutputType(typeof(AzureStorageBlob))]
    public class StartCopyAzureStorageBlob : StorageDataMovementCmdletBase
    {
        /// <summary>
        /// Blob Pipeline parameter set name
        /// </summary>
        private const string SrcBlobParameterSet = "BlobPipeline";

        /// <summary>
        /// Blob Pipeline parameter set name
        /// </summary>
        private const string DestBlobPipelineParameterSet = "DestBlobPipeline";

        /// <summary>
        /// Container pipeline paremeter set name
        /// </summary>
        private const string ContainerPipelineParmeterSet = "ContainerPipeline";

        /// <summary>
        /// Blob name and container name parameter set
        /// </summary>
        private const string NameParameterSet = "NamePipeline";

        /// <summary>
        /// Source uri parameter set
        /// </summary>
        private const string UriParameterSet = "UriPipeline";

        [Parameter(HelpMessage = "ICloudBlob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = SrcBlobParameterSet)]
        [Parameter(HelpMessage = "ICloudBlob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = DestBlobPipelineParameterSet)]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "CloudBlobContainer Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(ParameterSetName = ContainerPipelineParmeterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        [Parameter(ParameterSetName = NameParameterSet, Mandatory = true, Position = 0, HelpMessage = "Blob name")]
        public string SrcBlob
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        private string BlobName = String.Empty;

        [Parameter(HelpMessage = "Container name", Mandatory = true, Position = 1,
            ParameterSetName = NameParameterSet)]
        [ValidateNotNullOrEmpty]
        public string SrcContainer
        {
            get { return ContainerName; }
            set { ContainerName = value; }
        }
        private string ContainerName = String.Empty;

        [Parameter(HelpMessage = "Force to remove the blob and its snapshot without confirm")]
        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }
        private bool force = false;

        [Parameter(HelpMessage = "source blob uri", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = UriParameterSet)]
        public string SrcUri { get; set; }

        [Parameter(HelpMessage = "Destination container name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = NameParameterSet)]
        [Parameter(HelpMessage = "Destination container name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = UriParameterSet)]
        [Parameter(HelpMessage = "Destination container name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = SrcBlobParameterSet)]
        [Parameter(HelpMessage = "Destination container name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public string DestContainer { get; set; }

        [Parameter(HelpMessage = "Destination blob name", Mandatory = false,
            ValueFromPipelineByPropertyName = true, ParameterSetName = NameParameterSet)]
        [Parameter(HelpMessage = "Destination blob name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = UriParameterSet)]
        [Parameter(HelpMessage = "Destination blob name", Mandatory = false,
            ValueFromPipelineByPropertyName = true, ParameterSetName = SrcBlobParameterSet)]
        [Parameter(HelpMessage = "Destination container name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public string DestBlob { get; set; }

        [Parameter(HelpMessage = "Destination ICloudBlob object", Mandatory = true,
            ParameterSetName = DestBlobPipelineParameterSet)]
        public ICloudBlob DestICloudBlob { get; set; }

        [Alias("SrcContext")]
        [Parameter(HelpMessage = "Source Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true, ParameterSetName = NameParameterSet)]
        [Parameter(HelpMessage = "Source Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true, ParameterSetName = SrcBlobParameterSet)]
        [Parameter(HelpMessage = "Source Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true, ParameterSetName = DestBlobPipelineParameterSet)]
        [Parameter(HelpMessage = "Source Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerPipelineParmeterSet)]
        public override AzureStorageContext Context { get; set; }

        [Parameter(HelpMessage = "Destination Storage context object", Mandatory = false)]
        public AzureStorageContext DestContext { get; set; }

        /// <summary>
        /// Destination Service Channel object
        /// </summary>
        private IStorageBlobManagement destChannel;

        /// <summary>
        /// copy id for the current copy operation
        /// </summary>
        private string currentCopyId;

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            if(destChannel == null)
            {
                if (DestContext == null)
                {
                    destChannel = Channel;
                }
                else
                {
                    destChannel = CreateChannel(DestContext.StorageAccount);
                }
            }

            ICloudBlob destinationBlob = default(ICloudBlob);

            switch (ParameterSetName)
            {
                case NameParameterSet:
                    destinationBlob = StartCopyBlob(SrcContainer, SrcBlob, DestContainer, DestBlob);
                    break;

                case UriParameterSet:
                    destinationBlob = StartCopyBlob(SrcUri, DestContainer, DestBlob);
                    break;

                case SrcBlobParameterSet:
                    destinationBlob = StartCopyBlob(ICloudBlob, DestContainer, DestBlob);
                    break;

                case ContainerPipelineParmeterSet:
                    destinationBlob = StartCopyBlob(CloudBlobContainer.Name, SrcBlob, DestContainer, DestBlob);
                    break;

                case DestBlobPipelineParameterSet:
                    destinationBlob = StartCopyBlob(ICloudBlob, DestICloudBlob);
                    break;
            }

            if (destinationBlob != null)
            {
                AccessCondition accessCondition = null;
                BlobRequestOptions options = null;
                //Make sure we use the dest channel
                destChannel.FetchBlobAttributes(destinationBlob, accessCondition, options, OperationContext);
                AzureStorageBlob azureBlob = new AzureStorageBlob(destinationBlob);
                //Make sure the dest context is piped out
                azureBlob.Context = DestContext;
                WriteObject(azureBlob);
            }
        }

        /// <summary>
        /// Start copy operation by source and destination ICloudBlob object
        /// </summary>
        /// <param name="srcICloudBlob">Source ICloudBlob object</param>
        /// <param name="destICloudBlob">Destination ICloudBlob object</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyBlob(ICloudBlob srcICloudBlob, ICloudBlob destICloudBlob)
        {
            return StartCopyInTransferManager(srcICloudBlob, destICloudBlob.Container, destICloudBlob.Name);
        }

        /// <summary>
        /// Start copy operation by source ICloudBlob object
        /// </summary>
        /// <param name="srcICloudBlob">Source ICloudBlob object</param>
        /// <param name="destContainer">Destinaion container name</param>
        /// <param name="destBlobName">Destination blob name</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyBlob(ICloudBlob srcICloudBlob, string destContainer, string destBlobName)
        {
            CloudBlobContainer container = destChannel.GetContainerReference(destContainer);
            return StartCopyInTransferManager(srcICloudBlob, container, destBlobName);
        }

        /// <summary>
        /// Start copy operation by source uri
        /// </summary>
        /// <param name="srcICloudBlob">Source uri</param>
        /// <param name="destContainer">Destinaion container name</param>
        /// <param name="destBlobName">Destination blob name</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyBlob(string srcUri, string destContainer, string destBlobName)
        {
            CloudBlobContainer container = destChannel.GetContainerReference(destContainer);
            return StartCopyInTransferManager(new Uri(srcUri), container, destBlobName);   
        }

        /// <summary>
        /// Start copy operation by container name and blob name
        /// </summary>
        /// <param name="srcContainerName">Source container name</param>
        /// <param name="srcBlobName">Source blob name</param>
        /// <param name="destContainer">Destinaion container name</param>
        /// <param name="destBlobName">Destination blob name</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyBlob(string srcContainerName, string srcBlobName, string destContainerName, string destBlobName)
        {
            ValidateBlobName(srcBlobName);
            ValidateContainerName(srcContainerName);
            ValidateContainerName(destContainerName);

            if (string.IsNullOrEmpty(destBlobName))
            {
                destBlobName = srcBlobName;
            }

            ValidateBlobName(destBlobName);

            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            CloudBlobContainer container = Channel.GetContainerReference(srcContainerName);
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, srcBlobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, srcBlobName, srcContainerName));
            }

            CloudBlobContainer destContainer = destChannel.GetContainerReference(destContainerName);
            return StartCopyInTransferManager(blob, destContainer, destBlobName);
        }

        /// <summary>
        /// Start copy using transfer mangager by source ICloudBlob object
        /// </summary>
        /// <param name="blob">Source ICloudBlob object</param>
        /// <param name="destContainer">Destination CloudBlobContainer object</param>
        /// <param name="destBlobName">Destination blob name</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyInTransferManager(ICloudBlob blob, CloudBlobContainer destContainer, string destBlobName)
        {
            if (string.IsNullOrEmpty(destBlobName))
            {
                destBlobName = blob.Name;
            }

            ValidateBlobName(blob.Name);
            ValidateContainerName(destContainer.Name);
            ValidateBlobName(destBlobName);

            Action<BlobTransferManager> taskAction = (transferManager) => transferManager.QueueBlobStartCopy(blob, destContainer, destBlobName, null, OnTaskFinish, null);
            StartSyncTaskInTransferManager(taskAction);
            return GetDestinationBlobWithCopyId(destContainer, destBlobName, currentCopyId);
        }

        /// <summary>
        /// Start copy using transfer mangager by source uri
        /// </summary>
        /// <param name="uri">source uri</param>
        /// <param name="destContainer">Destination CloudBlobContainer object</param>
        /// <param name="destBlobName">Destination blob name</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob StartCopyInTransferManager(Uri uri, CloudBlobContainer destContainer, string destBlobName)
        {
            ValidateContainerName(destContainer.Name);
            ValidateBlobName(destBlobName);

            Action<BlobTransferManager> taskAction = (transferManager) => transferManager.QueueBlobStartCopy(uri, destContainer, destBlobName, null, OnTaskFinish, null);
            StartSyncTaskInTransferManager(taskAction);
            return GetDestinationBlobWithCopyId(destContainer, destBlobName, currentCopyId);
        }

        /// <summary>
        /// Get DestinationBlob with specified copy id
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="copyId">Current CopyId</param>
        /// <returns>Destination ICloudBlob object</returns>
        private ICloudBlob GetDestinationBlobWithCopyId(CloudBlobContainer container, string blobName, string copyId)
        {
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            ICloudBlob blob = destChannel.GetBlobReferenceFromServer(container, blobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                WriteObject(String.Format(Resources.CopyDestinationBlobPending, blobName, container.Name));
            }

            return blob;
        }

        private void OnCopyTaskFinish(object userData, Exception e, string copyId)
        {
            currentCopyId = copyId; //Make sure set the copy id before task finish
            OnTaskFinish(userData, e);
        }
    }
}
