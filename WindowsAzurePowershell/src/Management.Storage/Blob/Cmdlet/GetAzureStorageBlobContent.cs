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
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;

    [Cmdlet(VerbsCommon.Get, StorageNouns.BlobContent, DefaultParameterSetName = ManuallyParameterSet),
        OutputType(typeof(AzureStorageBlob))]
    public class GetAzureStorageBlobContentCommand : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// manually set the name parameter
        /// </summary>
        private const string ManuallyParameterSet = "ReceiveManual";

        /// <summary>
        /// blob pipeline
        /// </summary>
        private const string BlobParameterSet = "BlobPipeline";

        /// <summary>
        /// container pipeline
        /// </summary>
        private const string ContainerParameterSet = "ContainerPipeline";

        [Parameter(HelpMessage = "Azure Blob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = BlobParameterSet)]
        [ValidateNotNull]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "Azure Container Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true, ParameterSetName = ContainerParameterSet)]
        [ValidateNotNull]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(Position = 0, HelpMessage = "Blob name",
            Mandatory = true, ParameterSetName = ManuallyParameterSet)]
        [Parameter(Position = 0, HelpMessage = "Blob name",
            Mandatory = true, ParameterSetName = ContainerParameterSet)]
        public string Blob
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        private string BlobName = String.Empty;

        [Parameter(Position = 1, HelpMessage = "Container name",
            Mandatory = true, ParameterSetName = ManuallyParameterSet)]
        public string Container 
        {
            get { return ContainerName; }
            set { ContainerName = value; }
        }
        private string ContainerName = String.Empty;

        [Parameter(HelpMessage = "fileName")]
        public string File 
        {
            get { return FileName; }
            set { FileName = value; }
        }
        public string FileName = String.Empty;

        [Parameter(HelpMessage = "Force to overwrite the already existing local file")]
        public SwitchParameter Force
        {
            get { return overwrite; }
            set { overwrite = value; }
        }
        private bool overwrite;

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
        /// Initializes a new instance of the GetAzureStorageBlobContentCommand class.
        /// </summary>
        public GetAzureStorageBlobContentCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureStorageBlobContentCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public GetAzureStorageBlobContentCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// on download start
        /// </summary>
        /// <param name="progress">progress information</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnStart(object progress)
        {
            ProgressRecord pr = progress as ProgressRecord;

            if (null != pr)
            {
                pr.PercentComplete = 0;
            }
        }

        /// <summary>
        /// on downloading 
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="speed">download speed</param>
        /// <param name="percent">download percent</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnProgress(object progress, double speed, double percent)
        {
            ProgressRecord pr = progress as ProgressRecord;

            if (null == pr)
            {
                return;
            }
            
            int intPercent = (int)percent;
            
            if (intPercent > 100)
            {
                intPercent = 100;
            }
            else if(intPercent < 0)
            {
                intPercent = 0;
            }
            
            pr.PercentComplete = intPercent;
            pr.StatusDescription = String.Format(Resources.FileTransmitStatus, intPercent, speed);
        }

        /// <summary>
        /// whether the download progress finished
        /// </summary>
        private bool finished = false;

        /// <summary>
        /// on downloading finish
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="e">run time exception</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnFinish(object progress, Exception e)
        {
            finished = true;

            ProgressRecord pr = progress as ProgressRecord;
            
            if (null == pr)
            {
                return;
            }
            
            pr.PercentComplete = 100;
            
            if (null == e)
            {
                pr.StatusDescription = String.Format(Resources.DownloadBlobSuccessful, BlobName);
            }
            else
            {
                pr.StatusDescription = String.Format(Resources.DownloadBlobFailed, BlobName, ContainerName, FileName, e.Message);
            }
        }

        /// <summary>
        /// download blob to lcoal file
        /// </summary>
        /// <param name="blob">source blob object</param>
        /// <param name="filePath">destionation file path</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void DownloadBlob(ICloudBlob blob, string filePath)
        {
            int id = 0;
            string activity = String.Format(Resources.ReceiveAzureBlobActivity, blob.Name, filePath);
            string status = Resources.PrepareUploadingBlob;
            ProgressRecord pr = new ProgressRecord(id, activity, status);

            finished = false;
            pr.PercentComplete = 0;
            pr.StatusDescription = status;
            WriteProgress(pr);

            //status update interval
            int interval = 1 * 1000; //in millisecond

            BlobTransferOptions opts = new BlobTransferOptions();
            opts.ThreadCount = Environment.ProcessorCount * AsyncTasksPerCodeMultiplier;

            using (BlobTransferManager transferManager = new BlobTransferManager(opts))
            {
                transferManager.QueueDownload(blob, filePath, OnStart, OnProgress, OnFinish, pr);

                while (!finished)
                {
                    WriteProgress(pr);
                    System.Threading.Thread.Sleep(interval);
                }
                
                transferManager.WaitForCompletion();
            }
        }

        /// <summary>
        /// get blob content
        /// </summary>
        /// <param name="containerName">source container name</param>
        /// <param name="blobName">source blob name</param>
        /// <param name="fileName">file name</param>
        /// <returns>the downloaded AzureStorageBlob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob GetBlobContent(string containerName, string blobName, string fileName)
        {
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            return GetBlobContent(container, blobName, fileName);
        }

        /// <summary>
        /// get blob content
        /// </summary>
        /// <param name="container">source container object</param>
        /// <param name="blobName">source blob name</param>
        /// <param name="fileName">destionation file name</param>
        /// <returns>the downloaded AzureStorageBlob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob GetBlobContent(CloudBlobContainer container, string blobName, string fileName)
        {
            if (!NameUtil.IsValidBlobName(blobName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidBlobName, blobName));
            }

            if (!String.IsNullOrEmpty(fileName) && !NameUtil.IsValidFileName(fileName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidFileName, fileName));
            }

            ValidatePipelineCloudBlobContainer(container);
            AccessCondition accessCondition = null;
            BlobRequestOptions requestOptions = null;
            ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, requestOptions, OperationContext);
            
            if (null == blob)
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, container.Name));
            }

            return GetBlobContent(blob, fileName, true);
        }

        /// <summary>
        /// get blob content
        /// </summary>
        /// <param name="blob">source ICloudBlob object</param>
        /// <param name="fileName">destination file path</param>
        /// <param name="isValidBlob">whether the source container validated</param>
        /// <returns>the downloaded AzureStorageBlob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob GetBlobContent(ICloudBlob blob, string fileName, bool isValidBlob = false)
        {
            if (null == blob)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            }

            string filePath = Path.Combine(CurrentPath(), fileName);

            if (string.IsNullOrEmpty(fileName) || Directory.Exists(filePath))
            {
                fileName = blob.Name;
                filePath = Path.Combine(filePath, fileName);
            }

            fileName = Path.GetFileName(filePath);

            if (!NameUtil.IsValidFileName(fileName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidFileName, fileName));
            }

            String dirPath = Path.GetDirectoryName(filePath);

            if (!String.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                throw new ArgumentException(String.Format(Resources.DirectoryNotExists, dirPath));
            }

            if (!overwrite && System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(String.Format(Resources.FileAlreadyExists, filePath));
            }

            if (!isValidBlob)
            {
                ValidatePipelineICloudBlob(blob);
            }

            try
            {
                DownloadBlob(blob, filePath);
            }
            catch (Exception e)
            {
                WriteDebugLog(String.Format(Resources.DownloadBlobFailed, blob.Name, blob.Container.Name, filePath, e.Message));
                throw;
            }

            return new AzureStorageBlob(blob);
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureStorageBlob azureBlob = null;

            switch (ParameterSetName)
            {
                case BlobParameterSet:
                    azureBlob = GetBlobContent(ICloudBlob, FileName, false);
                    break;

                case ContainerParameterSet:
                    azureBlob = GetBlobContent(CloudBlobContainer, BlobName, FileName);
                    break;

                case ManuallyParameterSet:
                    azureBlob = GetBlobContent(ContainerName, BlobName, FileName);
                    break;
            }

            WriteObjectWithStorageContext(azureBlob);
        }
    }
}
