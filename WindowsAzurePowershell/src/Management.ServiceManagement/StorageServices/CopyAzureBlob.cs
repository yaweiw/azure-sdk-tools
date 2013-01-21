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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.StorageServices
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Sync.Download;

    [Cmdlet(VerbsCommon.Copy, "AzureBlob")]
    public class CopyAzureBlobCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public CopyAzureBlobCommand()
        {
        }

        public CopyAzureBlobCommand(IServiceManagement channel)
        {
            this.Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Specifies the URI of the source blob.")]
        [ValidateNotNullOrEmpty]
        [Alias("Src")]
        public Uri Source
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "Specifies the destination URI.")]
        [ValidateNotNullOrEmpty]
        [Alias("Dest")]
        public Uri Destination
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Indicates whether the blob at the destination URI should be overwritten if it exists.")]
        public SwitchParameter Overwrite
        {
            get { return this.overwrite; }
            set { this.overwrite = value; }
        }
        private bool overwrite = false;

        /// <summary>
        /// Reference to the destination blob.
        /// </summary>
        private ICloudBlob destBlob;

        /// <summary>
        /// Reference to the source blob .
        /// </summary>
        private ICloudBlob sourceBlob;

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        /// <summary>
        /// Stops processing records when the user stops the cmdlet asynchronously.
        /// </summary>
        protected override void StopProcessing()
        {
            this.AbortCommand();
        }

        /// <summary>
        /// Executes the Copy-AzureBlob Cmdlet.
        /// </summary>
        internal void ExecuteCommand()
        {
            if (this.Channel == null)
            {
                throw new ArgumentException("Could not copy blob. You may need to import publish settings.");
            }

            this.WriteOperationStatus("Validating");

            // Parse and validate the source info.
            try
            {
                BlobUri sourceUri;
                if (!BlobUri.TryParseUri(this.Source, out sourceUri) || string.IsNullOrEmpty(sourceUri.StorageAccountName))
                {
                    throw new ArgumentException(string.Format("Source blob Uri {0} is invalid.", this.Source.ToString()), "sourceBlobUri");
                }

                var sourceAccountKeys = this.Channel.GetStorageKeys(this.CurrentSubscription.SubscriptionId, sourceUri.StorageAccountName).StorageServiceKeys;
                var sourceCredentials = new StorageCredentials(sourceUri.StorageAccountName, sourceAccountKeys.Primary);
                var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
                var sourceClient = sourceAccount.CreateCloudBlobClient();
                var sourceContainer = sourceClient.GetContainerReference(sourceUri.BlobContainerName);

                if (sourceContainer.GetPermissions().PublicAccess != BlobContainerPublicAccessType.Blob)
                {
                    var sharedAccessPolicy = new SharedAccessBlobPolicy() {Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1)};
                    var sourceSas = sourceContainer.GetSharedAccessSignature(sharedAccessPolicy);
                    var sourceSasCreds = new StorageCredentials(sourceSas);
                    var sourceSasClient = new CloudBlobClient(sourceAccount.BlobEndpoint, sourceSasCreds);
                    sourceContainer = sourceSasClient.GetContainerReference(sourceUri.BlobContainerName);
                }

                sourceBlob = sourceContainer.GetBlobReferenceFromServer(sourceUri.BlobName);
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ArgumentException(string.Format("Source Uri {0} could not be found.", this.Source.ToString()), "sourceBlobUri");
            }

            if (!this.BlobExists(sourceBlob))
            {
                throw new ArgumentException(string.Format("Source blob {0} doesn't exist. Not copying", this.Source.AbsoluteUri), "sourceBlob");
            }

            // Parse and validate the destination info.
            try
            {
                BlobUri destUri;
                if (!BlobUri.TryParseUri(this.Destination, out destUri) || string.IsNullOrEmpty(destUri.StorageAccountName))
                {
                    throw new ArgumentException(string.Format("Destination blob Uri {0} is invalid.", this.Destination.ToString()), "destBlobUri");
                }

                var destAccountKeys = this.Channel.GetStorageKeys(this.CurrentSubscription.SubscriptionId, destUri.StorageAccountName).StorageServiceKeys;
                var destCredentials = new StorageCredentials(destUri.StorageAccountName, destAccountKeys.Primary);
                var destAccount = new CloudStorageAccount(destCredentials, false);
                var destClient = destAccount.CreateCloudBlobClient();
                var destContainer = destClient.GetContainerReference(destUri.BlobContainerName);
                destContainer.CreateIfNotExists();

                try
                {
                    destBlob = destContainer.GetBlobReferenceFromServer(destUri.BlobName);

                    if (destBlob.BlobType != sourceBlob.BlobType)
                    {
                        destBlob.Delete();
                        destBlob = this.GetBlobReference(destContainer, destUri.BlobName, sourceBlob.BlobType);
                    }
                }
                catch (StorageException ex2)
                {
                    if (ex2.RequestInformation.HttpStatusCode == 404)
                    {
                        // The dest blob does not exist so we need to get our own reference.
                        destBlob = this.GetBlobReference(destContainer, destUri.BlobName, sourceBlob.BlobType);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (EndpointNotFoundException ex)
            {
                throw new ArgumentException(string.Format("Destination Uri {0} could not be found.", this.Destination.ToString()), "destBlobUri");
            }

            // Begin copying
            if (this.BlobExists(destBlob))
            {
                if(!this.Overwrite.IsPresent)
                {
                    throw new ArgumentException(string.Format("Destination blob {0} exists. Not copying.", this.Destination.AbsoluteUri), "destBlob");
                }

                // Begin copying blob, overwriting existing blob, or resume monitoring the copy already in progress.
                if (destBlob.CopyState.Status != CopyStatus.Pending)
                {
                    this.CopyBlob(sourceBlob, destBlob);
                }
                else
                {
                    this.WriteOperationStatus("Monitoring copy already in progress.");
                }
            }
            else
            {
                // Copy the blob
                this.CopyBlob(sourceBlob, destBlob);

                // Wait for the copy operation to start
                while (!this.BlobExists(destBlob))
                {
                    Thread.Sleep(100);
                }
            }

            // Get the progress and wait for the operation to finish
            do
            {
                Thread.Sleep(1000);
                destBlob.FetchAttributes();
                this.UpdateProgress(destBlob.CopyState.BytesCopied, destBlob.CopyState.TotalBytes);
            } 
            while (destBlob.CopyState.Status == CopyStatus.Pending && !this.Stopping);

            if (!this.Stopping)
            {
                this.WriteOperationStatus("Complete");
                this.WriteObject(new CopyBlobContext() { DestinationUri = this.Destination });
            }
        }

        /// <summary>
        /// Aborts the execution of the Copy-AzureBlob Cmdlet.
        /// </summary>
        internal void AbortCommand()
        {
            if (this.destBlob != null)
            {
                this.destBlob.FetchAttributes();
                if (!string.IsNullOrEmpty(this.destBlob.CopyState.CopyId))
                {
                    this.destBlob.AbortCopy(this.destBlob.CopyState.CopyId);
                }
            }

            base.StopProcessing();
        }

        /// <summary>
        /// Gets whether the given blob exists.
        /// </summary>
        /// <param name="blob">The blob to check if it exists.</param>
        /// <returns>True if the blob exists.</returns>
        private bool BlobExists(ICloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return blob.Exists();
            }
            catch (StorageException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Copies a blob.
        /// </summary>
        /// <param name="source">The source blob.</param>
        /// <param name="destination">The destination blob.</param>
        private void CopyBlob(ICloudBlob source, ICloudBlob destination)
        {
            switch (source.BlobType)
            {
                case BlobType.BlockBlob:
                    ((CloudBlockBlob)destination).StartCopyFromBlob((CloudBlockBlob)source);
                    break;
                case BlobType.PageBlob:
                    ((CloudPageBlob)destination).StartCopyFromBlob((CloudPageBlob)source);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported block blob type '{0}'.", source.BlobType.ToString()));
            }
        }

        /// <summary>
        /// Gets a reference to the given blob.
        /// </summary>
        /// <param name="container">The blob's parent container.</param>
        /// <param name="blobName">The blob's name.</param>
        /// <param name="blobType">The blob's blob type.</param>
        /// <returns>Returns a reference to the given blob.</returns>
        private ICloudBlob GetBlobReference(CloudBlobContainer container, string blobName, BlobType blobType)
        {
            ICloudBlob ret;

            switch (blobType)
            {
                case BlobType.BlockBlob:
                    ret = container.GetBlockBlobReference(blobName);
                    break;
                case BlobType.PageBlob:
                    ret = container.GetPageBlobReference(blobName);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported block blob type '{0}'.", blobType.ToString()));
            }

            return ret;
        }

        /// <summary>
        /// Updates the progress in the operation status.
        /// </summary>
        /// <param name="bytesCopied">The number of bytes that have already been copied.</param>
        /// <param name="totalBytes">The total number of bytes to copy.</param>
        private void UpdateProgress(long? bytesCopied, long? totalBytes)
        {
            double p = bytesCopied ?? 0.0;
            double t = totalBytes ?? 1.0;
            this.WriteOperationStatus(string.Format("{0:0.}%", (p / t) * 100.0));
        }

        /// <summary>
        /// Writes the current status of the copy operation.
        /// </summary>
        /// <param name="status">Status string.</param>
        private void WriteOperationStatus(string status)
        {
            this.WriteProgress(new ProgressRecord(1, "Copy-AzureBlob", string.Format("Operation Status: {0}", status)));
        }
    }
}
