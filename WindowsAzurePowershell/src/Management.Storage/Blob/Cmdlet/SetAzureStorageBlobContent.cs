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
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// download blob from azure
    /// </summary>
    [Cmdlet(VerbsCommon.Set, StorageNouns.BlobContent, ConfirmImpact = ConfirmImpact.High, DefaultParameterSetName = ManuallyParameterSet),
        OutputType(typeof(AzureStorageBlob))]
    public class SetAzureBlobContentCommand : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// default parameter set name
        /// </summary>
        private const string ManuallyParameterSet = "SendManual";

        /// <summary>
        /// blob pipeline
        /// </summary>
        private const string BlobParameterSet = "BlobPipeline";

        /// <summary>
        /// container pipeline
        /// </summary>
        private const string ContainerParameterSet = "ContainerPipeline";

        /// <summary>
        /// block blob type
        /// </summary>
        private const string BlockBlobType = "block";

        /// <summary>
        /// page blob type
        /// </summary>
        private const string PageBlobType = "page";

        [Alias("FullName")]
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "file Path",
            ValueFromPipelineByPropertyName = true, ParameterSetName = ManuallyParameterSet)]
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "file Path",
            ParameterSetName = ContainerParameterSet)]
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "file Path",
            ParameterSetName = BlobParameterSet)]
        public string File
        {
            get { return FileName; }
            set { FileName = value; }
        }
        private string FileName = String.Empty;

        [Parameter(Position = 1, HelpMessage = "Container name", Mandatory = true, ParameterSetName = ManuallyParameterSet)]
        public string Container
        {
            get { return ContainerName; }
            set { ContainerName = value; }
        }
        private string ContainerName = String.Empty;

        [Parameter(HelpMessage = "Blob name", ParameterSetName = ManuallyParameterSet)]
        [Parameter(HelpMessage = "Blob name", ParameterSetName = ContainerParameterSet)]
        public string Blob
        {
            get { return BlobName; }
            set { BlobName = value; }
        }
        public string BlobName = String.Empty;

        [Parameter(HelpMessage = "Azure Blob Container Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ContainerParameterSet)]
        public CloudBlobContainer CloudBlobContainer { get; set; }

        [Parameter(HelpMessage = "Azure Blob Object", Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = BlobParameterSet)]
        public ICloudBlob ICloudBlob { get; set; }

        [Parameter(HelpMessage = "Blob Type('block', 'page')")]
        [ValidateSet(BlockBlobType, PageBlobType)]
        public string BlobType
        {
            get { return blobType; }
            set { blobType = value; }
        }
        private string blobType = BlockBlobType;

        [Parameter(HelpMessage = "Force to overwrite the already existing blob")]
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
        /// Initializes a new instance of the SetAzureBlobContentCommand class.
        /// </summary>
        public SetAzureBlobContentCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SetAzureBlobContentCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public SetAzureBlobContentCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// on uploading start
        /// </summary>
        /// <param name="progress">process infomation</param>
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
        /// on uploading
        /// </summary>
        /// <param name="progress">process infomation</param>
        /// <param name="speed">upload speed</param>
        /// <param name="percent">upload percent</param>
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
            else if (intPercent < 0)
            {
                intPercent = 0;
            }
            
            pr.PercentComplete = intPercent;            
            pr.StatusDescription = String.Format(Resources.FileTransmitStatus, intPercent, speed);
        }

        /// <summary>
        /// whether the uploading finished
        /// </summary>
        private bool finished = false;

        /// <summary>
        /// on uploading finish
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
                pr.StatusDescription = String.Format(Resources.UploadFileSuccessfully, FileName);
            }
            else
            {
                pr.StatusDescription = String.Format(Resources.UploadFileFailed, FileName, e.Message);
            }
        }

        /// <summary>
        /// upload file to azure blob
        /// </summary>
        /// <param name="filePath">local file path</param>
        /// <param name="blob">destination azure blob object</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void Upload2Blob(string filePath, ICloudBlob blob)
        {
            int id = 0;
            string activity = String.Format(Resources.SendAzureBlobActivity, filePath, blob.Name, blob.Container.Name);
            string status = Resources.PrepareUploadingBlob;
            ProgressRecord pr = new ProgressRecord(id, activity, status);

            finished = false;
            pr.PercentComplete = 0;
            pr.StatusDescription = status;

            WriteProgress(pr);
            
            BlobTransferOptions opts = new BlobTransferOptions();
            opts.ThreadCount = Environment.ProcessorCount * AsyncTasksPerCodeMultiplier;
            
            //status update interval
            int interval = 1 * 1000; //in millisecond
            
            using (BlobTransferManager transferManager = new BlobTransferManager(opts))
            {
                transferManager.QueueUpload(blob, blob.BlobType, filePath,
                    true, OnStart, OnProgress, OnFinish, pr);

                while (!finished)
                {
                    WriteProgress(pr);
                    System.Threading.Thread.Sleep(interval);
                }

                transferManager.WaitForCompletion();
            }
        }

        /// <summary>
        /// confirm the overwrite operatioin
        /// </summary>
        /// <param name="msg">confirmation message</param>
        /// <returns>true if the opeation is confirmed, otherwise return false</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual bool ConfirmOverwrite(string msg = null)
        {
            if (String.IsNullOrEmpty(msg))
            {
                msg = BlobName;
            }

            return ShouldProcess(msg);
        }

        /// <summary>
        /// get full file path according to the specified file name
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns>full file path</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal string GetFullSendFilePath(string fileName)
        {
            String filePath = Path.Combine(CurrentPath(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                if (System.IO.Directory.Exists(filePath))
                {
                    throw new ArgumentException(String.Format(Resources.CannotSendDirectory, filePath));
                }
                else
                {
                    throw new ArgumentException(String.Format(Resources.FileNotFound, filePath));
                }
            }

            return filePath;
        }

        /// <summary>
        /// set azure blob content
        /// </summary>
        /// <param name="fileName">local file path</param>
        /// <param name="containerName">container name</param>
        /// <param name="blobName">blob name</param>
        /// <returns>null if user cancel the overwrite operation, otherwise return destionation blob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob SetAzureBlobContent(string fileName, string containerName, string blobName)
        {
            CloudBlobContainer container = Channel.GetContainerReference(containerName);
            return SetAzureBlobContent(fileName, container, blobName);
        }

        /// <summary>
        /// set azure blob content
        /// </summary>
        /// <param name="fileName">local file path</param>
        /// <param name="container">destination container</param>
        /// <param name="blobName">blob name</param>
        /// <returns>null if user cancel the overwrite operation, otherwise return destionation blob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob SetAzureBlobContent(string fileName, CloudBlobContainer container, string blobName)
        {
            string filePath = GetFullSendFilePath(fileName);

            ValidatePipelineCloudBlobContainer(container);

            if (string.IsNullOrEmpty(blobName))
            {
                blobName = Path.GetFileName(filePath);
            }

            ICloudBlob blob = null;

            switch (blobType.ToLower())
            {
                case PageBlobType:
                    blob = container.GetPageBlobReference(blobName);
                    break;

                case BlockBlobType:
                default:
                    blob = container.GetBlockBlobReference(blobName);
                    break;
            }

            return SetAzureBlobContent(fileName, blob);
        }

        /// <summary>
        /// set azure blob
        /// </summary>
        /// <param name="fileName">local file name</param>
        /// <param name="blob">destination blob</param>
        /// <param name="isValidContainer">whether the destination container is validated</param>
        /// <returns>null if user cancel the overwrite operation, otherwise return destionation blob object</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal AzureStorageBlob SetAzureBlobContent(string fileName, ICloudBlob blob, bool isValidContainer = false)
        {
            string filePath = GetFullSendFilePath(fileName);

            if (null == blob)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            }

            if (!NameUtil.IsValidBlobName(blob.Name))
            {
                throw new ArgumentException(String.Format(Resources.InvalidBlobName, blob.Name));
            }

            if (!isValidContainer)
            {
                ValidatePipelineCloudBlobContainer(blob.Container);
            }

            AccessCondition accessCondition = null;
            BlobRequestOptions requestOptions = null; ;
            ICloudBlob blobRef = Channel.GetBlobReferenceFromServer(blob.Container, blob.Name, accessCondition, requestOptions, OperationContext);

            if (null != blobRef)
            {
                if (blob.BlobType != blobRef.BlobType)
                {
                    throw new ArgumentException(String.Format(Resources.BlobTypeMismatch, blobRef.Name, blobRef.BlobType));
                }
            
                if (!overwrite)
                {
                    if (!ConfirmOverwrite(blob.Name))
                    {
                        return null;
                    }
                }
            }

            try
            {
                Upload2Blob(filePath, blob);
            }
            catch (Exception e)
            {
                WriteDebugLog(String.Format(Resources.Upload2BlobFailed, e.Message));
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
            AzureStorageBlob blob = null;
            string blobName = string.Empty;

            switch (ParameterSetName)
            {
                case ContainerParameterSet:
                    blob = SetAzureBlobContent(FileName, CloudBlobContainer, BlobName);
                    blobName = BlobName;
                    break;
                case BlobParameterSet:
                    blob = SetAzureBlobContent(FileName, ICloudBlob);
                    blobName = ICloudBlob.Name;
                    break;
                case ManuallyParameterSet:
                default:
                    blob = SetAzureBlobContent(FileName, ContainerName, BlobName);
                    blobName = blob.Name;
                    break;
            }

            if (blob == null)
            {
                String result = String.Format(Resources.SendAzureBlobCancelled, FileName, blobName);
                WriteObject(result);
            }
            else
            {
                WriteObjectWithStorageContext(blob);
            }
        }
    }
}
