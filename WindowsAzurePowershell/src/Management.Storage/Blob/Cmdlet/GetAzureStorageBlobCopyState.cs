using Microsoft.WindowsAzure.Management.Storage.Common;
using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    [Cmdlet(VerbsCommon.Get, StorageNouns.CopyBlobStatus, DefaultParameterSetName = NameParameterSet),
       OutputType(typeof(AzureStorageBlob))]
    public class GetAzureStorageBlobCopyState : StorageCloudBlobCmdletBase
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

        [Parameter(HelpMessage = "Wait for copy task complete")]
        public SwitchParameter WaitForComplete
        {
            get { return waitForComplete;}
            set { waitForComplete = value; }
        }
        private bool waitForComplete;

        private List<ICloudBlob> jobList = new List<ICloudBlob>();
        private int total = 0;
        private int failed = 0;
        private int finished = 0;

        public override void ExecuteCmdlet()
        {
            ICloudBlob blob = default(ICloudBlob);
            switch (ParameterSetName)
            {
                case NameParameterSet:
                    blob = GetBlobWithCopyStatus(ContainerName, BlobName);
                    break;
                case ContainerPipelineParmeterSet:
                    blob = GetBlobWithCopyStatus(CloudBlobContainer, BlobName);
                    break;
                case BlobPipelineParameterSet:
                    blob = GetBlobWithCopyStatus(ICloudBlob);
                    break;
            }

            total++;

            if (blob.CopyState == null)
            {
                throw new ArgumentException(String.Format(Resources.CopyTaskNotFound, blob.Name, blob.Container.Name));
            }
            else
            {
                UpdateTaskCount(blob.CopyState.Status);

                if (blob.CopyState.Status == CopyStatus.Pending && waitForComplete)
                {
                    jobList.Add(blob);
                }
                else
                {
                    WriteCopyState(blob);
                }
            }
        }

        private void UpdateTaskCount(CopyStatus status)
        {
            switch (status)
            {
                case CopyStatus.Invalid:
                case CopyStatus.Failed:
                case CopyStatus.Aborted:
                    failed++;
                    break;
                case CopyStatus.Pending:
                    break;
                case CopyStatus.Success:
                default:
                    finished++;
                    break;
            }
        }

        internal void WriteCopyState(ICloudBlob blob)
        {
            WriteObject(blob.CopyState);
        }

        internal void WriteCopyProgress(ICloudBlob blob, ProgressRecord progress)
        {
            long bytesCopied = blob.CopyState.BytesCopied ?? 0;
            long totalBytes = blob.CopyState.TotalBytes ?? 0;
            int percent = 0;

            if (totalBytes != 0)
            {
                percent = (int)(bytesCopied * 100 / totalBytes);
                progress.PercentComplete = percent;
            }

            string activity = String.Format(Resources.CopyBlobStatus, blob.CopyState.Status.ToString(), blob.Name, blob.Container.Name, blob.CopyState.Source.ToString());
            progress.Activity = activity;
            string message = String.Format(Resources.CopyBlobPendingStatus, percent, blob.CopyState.BytesCopied, blob.CopyState.TotalBytes);
            progress.StatusDescription = message;
            WriteProgress(progress);
        }

        private ICloudBlob GetBlobWithCopyStatus(string containerName, string blobName)
        {
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            return GetBlobWithCopyStatus(container, blobName);
        }

        private ICloudBlob GetBlobWithCopyStatus(CloudBlobContainer container, string blobName)
        {
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;

            ValidateBlobName(blobName);
            ValidateContainerName(container.Name);

            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, options, OperationContext);

            if (blob == null)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, container.Name));
            }

            return GetBlobWithCopyStatus(blob);
        }

        private ICloudBlob GetBlobWithCopyStatus(ICloudBlob blob)
        {
            ValidateBlobName(blob.Name);

            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            Channel.FetchBlobAttributes(blob, accessCondition, options, OperationContext);
            return blob;
        }

        protected override void EndProcessing()
        {
            if (jobList.Count >= 0)
            {
                List<ProgressRecord> records = new List<ProgressRecord>();
                int defaultTaskRecordCount = 5;
                string summary = String.Format(Resources.CopyBlobSummaryCount, total, finished, jobList.Count, failed);
                ProgressRecord summaryRecord = new ProgressRecord(0, Resources.CopyBlobSummaryActivity, summary);
                records.Add(summaryRecord);

                for (int i = 1; i <= defaultTaskRecordCount; i++)
                {
                    ProgressRecord record = new ProgressRecord(i, Resources.CopyBlobActivity, "Copy");
                    records.Add(record);
                }

                int workerPtr = 0;
                int taskRecordStartIndex = 1;

                while (jobList.Count > 0)
                {
                    summary = String.Format(Resources.CopyBlobSummaryCount, total, finished, jobList.Count, failed);
                    WriteProgress(summaryRecord);

                    for (int i = taskRecordStartIndex; i <= defaultTaskRecordCount && !ShouldForceQuit; i++)
                    {
                        ICloudBlob blob = jobList[workerPtr];
                        GetBlobWithCopyStatus(blob);
                        WriteCopyProgress(blob, records[i]);
                        UpdateTaskCount(blob.CopyState.Status);

                        if (blob.CopyState.Status != CopyStatus.Pending)
                        {
                            WriteCopyState(blob);
                            jobList.RemoveAt(workerPtr);
                        }
                        else
                        {
                            workerPtr++;
                        }

                        if (jobList.Count == 0)
                        {
                            break;
                        }

                        if (workerPtr >= jobList.Count)
                        {
                            workerPtr = 0;
                            break;
                        }
                    }

                    if (ShouldForceQuit)
                    {
                        break;
                    }
                    else
                    {
                        //status update interval
                        int interval = 1 * 1000; //in millisecond
                        Thread.Sleep(interval);
                    }
                }
            }

            base.EndProcessing();
        }
    }
}
