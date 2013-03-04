
namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Common.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Threading;

    [Cmdlet(VerbsLifecycle.Start, StorageNouns.CopyBlob, DefaultParameterSetName = NameParameterSet),
       OutputType(typeof(AzureStorageBlob))]
    public class StartCopyAzureStorageBlob : StorageCloudBlobCmdletBase
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
        public string destContainer { get; set; }

        [Parameter(HelpMessage = "Destination blob name", Mandatory = false,
            ValueFromPipelineByPropertyName = true, ParameterSetName = NameParameterSet)]
        [Parameter(HelpMessage = "Destination blob name", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = UriParameterSet)]
        [Parameter(HelpMessage = "Destination blob name", Mandatory = false,
            ValueFromPipelineByPropertyName = true, ParameterSetName = SrcBlobParameterSet)]
        public string DestBlob { get; set; }

        [Parameter(HelpMessage = "Destination ICloudBlob object", Mandatory = true,
            ParameterSetName = DestBlobPipelineParameterSet)]
        public ICloudBlob DestICloudBlob { get; set; }

        [Parameter(HelpMessage = "Destination Storage context object", Mandatory = false)]
        public AzureStorageContext DestContext { get; set; }

        private IStorageBlobManagement destChannel;

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

            switch (ParameterSetName)
            {
                case NameParameterSet:
                    StartCopyBlob(SrcContainer, SrcBlob, destContainer, DestBlob);
                    break;

                case UriParameterSet:
                    StartCopyBlob(SrcUri, destContainer, DestBlob);
                    break;

                case SrcBlobParameterSet:
                    StartCopyBlob(ICloudBlob, destContainer, DestBlob);
                    break;

                case DestBlobPipelineParameterSet:
                    StartCopyBlob(ICloudBlob, DestICloudBlob);
                    break;
            }
        }

        private void StartCopyBlob(ICloudBlob srcICloudBlob, ICloudBlob destICloudBlob)
        {
            StartCopyInTransferManager(srcICloudBlob, destICloudBlob.Container, destICloudBlob.Name);
        }

        private void StartCopyBlob(ICloudBlob srcICloudBlob, string destContainer, string destBlobName)
        {
            CloudBlobContainer container = destChannel.GetContainerReference(destContainer);
            StartCopyInTransferManager(srcICloudBlob, container, destBlobName);
        }

        private void StartCopyBlob(string srcUri, string destContainer, string destBlobName)
        {
            CloudBlobContainer container = destChannel.GetContainerReference(destContainer);
            StartCopyInTransferManager(new Uri(srcUri), container, destBlobName);   
        }

        private void StartCopyBlob(string srcContainerName, string srcBlobName, string destContainerName, string destBlobName)
        {
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            CloudBlobContainer container = Channel.GetContainerReference(srcContainerName);
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, srcBlobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, srcBlobName, srcContainerName));
            }

            CloudBlobContainer destContainer = destChannel.GetContainerReference(destContainerName);
            StartCopyInTransferManager(blob, destContainer, destBlobName);
        }

        /// <summary>
        /// Amount of concurrent async tasks to run per available core.
        /// </summary>
        [Alias("Concurrent")]
        [Parameter(HelpMessage = "Amount of concurrent async tasks to run per available core.")]
        public int ConcurrentCount
        {
            get { return AsyncTasksPerCodeMultiplier; }
            set { AsyncTasksPerCodeMultiplier = value; }
        }
        private int AsyncTasksPerCodeMultiplier = 8;

        /// <summary>
        /// whether the download progress finished
        /// </summary>
        private bool finished = false;

        /// <summary>
        /// exception thrown during downloading
        /// </summary>
        private Exception copyException = null;

        /// <summary>
        /// on uploading finish
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="e">run time exception</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnFinish(object data, Exception e)
        {
            finished = true;
            copyException = e;
        }

        private void StartCopyInTransferManager(ICloudBlob blob, CloudBlobContainer destContainer, string destBlobName)
        {
            if (string.IsNullOrEmpty(destBlobName))
            {
                destBlobName = blob.Name;
            }

            Action<BlobTransferManager> taskAction = (transferManager) => transferManager.QueueBlobStartCopy(blob, destContainer, destBlobName, null, OnFinish, null);
            StartCopyInTransferManager(taskAction, destContainer, destBlobName);
        }

        private void StartCopyInTransferManager(Uri uri, CloudBlobContainer destContainer, string destBlobName)
        {
            Action<BlobTransferManager> taskAction = (transferManager) => transferManager.QueueBlobStartCopy(uri, destContainer, destBlobName, null, OnFinish, null);
            StartCopyInTransferManager(taskAction, destContainer, destBlobName);
        }

        private void StartCopyInTransferManager(Action<BlobTransferManager> taskAction, CloudBlobContainer destContainer, string destBlobName)
        {
            finished = false;

            //status update interval
            int interval = 1 * 1000; //in millisecond

            BlobTransferOptions opts = new BlobTransferOptions();
            opts.Concurrency = Environment.ProcessorCount * AsyncTasksPerCodeMultiplier;

            using (BlobTransferManager transferManager = new BlobTransferManager(opts))
            {
                taskAction(transferManager);

                while (!finished)
                {
                    Thread.Sleep(interval);

                    if (ShouldForceQuit)
                    {
                        //can't output verbose log for this operation since the Output stream is already stopped.
                        transferManager.CancelWork();
                        break;
                    }
                }

                transferManager.WaitForCompletion();

                if (copyException != null)
                {
                    throw copyException;
                }
                else
                {
                    AccessCondition accessCondition = null;
                    BlobRequestOptions options = null;
                    ICloudBlob blob = destChannel.GetBlobReferenceFromServer(destContainer, destBlobName, accessCondition, options, OperationContext);
                    WriteICloudBlobWithProperties(blob, destChannel);
                }
            }
        }
    }
}
