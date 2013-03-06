using Microsoft.WindowsAzure.Management.Storage.Common;
using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    [Cmdlet(VerbsLifecycle.Stop, StorageNouns.CopyBlob, DefaultParameterSetName = NameParameterSet),
       OutputType(typeof(AzureStorageBlob))]
    public class StopCopyAzureStorageBlob : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// Blob Pipeline parameter set name
        /// </summary>
        private const string BlobPipelineParameterSet = "BlobPipeline";

        /// <summary>
        /// Container pipeline paremeter set name
        /// </summary>
        private const string ContainerPipelineParmeterSet = "ContainerPipeline";

        /// <summary>
        /// Blob name and container name parameter set
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

        [Parameter(HelpMessage = "Force to remove the blob and its snapshot without confirm")]
        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }
        private bool force = false;

        [Parameter(HelpMessage = "Copy Id", Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CopyId
        {
            get { return copyId; }
            set { copyId = value; }
        }
        private string copyId;

        public override void ExecuteCmdlet()
        {
            string blobName = string.Empty;
            string containerName = string.Empty;
            switch (ParameterSetName)
            { 
                case NameParameterSet:
                    StopCopyBlob(ContainerName, BlobName, copyId);
                    blobName = BlobName;
                    containerName = ContainerName;
                    break;
                case ContainerPipelineParmeterSet:
                    StopCopyBlob(CloudBlobContainer, BlobName, copyId);
                    blobName = BlobName;
                    containerName = CloudBlobContainer.Name;
                    break;
                case BlobPipelineParameterSet:
                    StopCopyBlob(ICloudBlob, copyId);
                    blobName = ICloudBlob.Name;
                    containerName = ICloudBlob.Container.Name;
                    break;
            }

            string message = String.Format(Resources.StopCopyBlobSuccessfully, blobName, containerName);
            WriteObject(message);
        }

        private void StopCopyBlob(string containerName, string blobName, string copyId)
        {            
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            StopCopyBlob(container, blobName, copyId);
        }

        private void StopCopyBlob(CloudBlobContainer container, string blobName, string copyId)
        {
            ValidateBlobName(blobName);

            ValidateContainerName(container.Name);

            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, container.Name));
            }

            StopCopyBlob(blob, copyId);
        }

        private void StopCopyBlob(ICloudBlob blob, string copyId)
        {
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;

            if (null == blob)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            }

            if (Force)
            {
                Channel.FetchBlobAttributes(blob, accessCondition, options, OperationContext);

                if (blob.CopyState != null && !String.IsNullOrEmpty(blob.CopyState.CopyId))
                {
                    copyId = blob.CopyState.CopyId;
                }
            }

            if (String.IsNullOrEmpty(copyId))
            {
                throw new ArgumentException(Resources.CopyIdCannotBeEmpty);
            }

            //TODO handle 400 copy id is invalid 
            //TODO handle 409 conflict Trying to abort a copy that has completed or failed results in 409 Conflict. Trying to abort a copy operation using an incorrect copy ID also results in 409 Conflict.
            Channel.AbortCopy(blob, copyId, accessCondition, options, OperationContext);
        }
    }
}
