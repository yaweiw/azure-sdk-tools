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
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Storage.Blob.Context;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Sync.Download;

    [Cmdlet(VerbsCommon.Copy, "AzureStorageBlob"), OutputType(typeof(CopyAzureStorageBlobContext))]
    public class CopyAzureStorageBlobCommand : CloudBaseCmdlet<IServiceManagement>
    {
        #region Fields

        /// <summary>
        /// Reference to the destination blob.
        /// </summary>
        private ICloudBlob destBlob;
        
        #endregion Fields

        #region Parameters

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

        [Parameter(Position = 2, Mandatory = false, HelpMessage = "Specified the storage key for the destination account.")]
        [Alias("Key")]
        public string DestinationStorageKey
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

        #endregion Parameters

        public CopyAzureStorageBlobCommand()
        {
        }

        public CopyAzureStorageBlobCommand(IServiceManagement channel)
        {
            this.Channel = channel;
        }

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
        /// Executes the Copy-AzureStorageBlob Cmdlet.
        /// </summary>
        internal void ExecuteCommand()
        {
            if (this.Channel == null)
            {
                throw new ArgumentException("Could not copy blob. You may need to import publish settings.");
            }

            this.WriteOperationStatus("Validating");

            // Validate the destination URI.
            BlobUri destUri;
            if (!BlobUri.TryParseUri(this.Destination, out destUri) 
                || string.IsNullOrEmpty(destUri.StorageAccountName))
            {
                throw new ArgumentException(
                    string.Format(
                        "Destination blob Uri {0} is invalid.", 
                        this.Destination.ToString()));
            }

            // Get destination storage credentials.
            StorageCredentials destCredentials;
            if (string.IsNullOrEmpty(this.DestinationStorageKey))
            {
                try
                {
                    var destAccountKeys = this.Channel.GetStorageKeys(this.CurrentSubscription.SubscriptionId, destUri.StorageAccountName).StorageServiceKeys;
                    destCredentials = new StorageCredentials(destUri.StorageAccountName, destAccountKeys.Primary);
                }
                catch (EndpointNotFoundException ex)
                {
                    // The destination Storage account was not found in the current subscription.
                    throw new ArgumentException(
                        string.Format(
                            "The storage account '{0}' was not found in the subscription '{1}'. Try specifying a storage account key.", 
                            destUri.StorageAccountName,
                            this.CurrentSubscription.SubscriptionName));
                }
            }
            else
            {
                destCredentials = new StorageCredentials(destUri.StorageAccountName, this.DestinationStorageKey);
            }

            // Get the destination blob container reference.
            CloudBlobContainer destContainer;
            try
            {
                var destAccount = new CloudStorageAccount(destCredentials, false);
                var destClient = destAccount.CreateCloudBlobClient();
                destContainer = destClient.GetContainerReference(destUri.BlobContainerName);
                destContainer.CreateIfNotExists();
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 502) // Bad Gateway.
                {
                    throw new ArgumentException(
                        string.Format(
                            "The storage account '{0}' was not found", 
                            destUri.StorageAccountName));
                }
                else if (ex.RequestInformation.HttpStatusCode == 403) // Forbidden
                {
                    throw new ArgumentException(
                        string.Format(
                            "The given storage account key was not valid for storage account '{0}.", 
                            destUri.StorageAccountName));
                }
                else
                {
                    throw;
                }
            }

            // Get the destination blob reference.
            try
            {
                destBlob = destContainer.GetBlobReferenceFromServer(destUri.BlobName);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404) // Not Found
                {
                    // The dest blob does not exist so we need to get our own reference.
                    destBlob = destContainer.GetBlockBlobReference(destUri.BlobName);
                }
                else
                {
                    throw;
                }
            }

            // Validate that we can perform the copy.
            bool monitorCopyInProgress = false;
            if(this.BlobExists(destBlob))
            {
                if (destBlob.CopyState != null && destBlob.CopyState.Status == CopyStatus.Pending)
                {
                    if (destBlob.CopyState.Source.Host == this.Source.Host
                        && destBlob.CopyState.Source.AbsolutePath == this.Source.AbsolutePath)
                    {
                        monitorCopyInProgress = true;
                        this.WriteOperationStatus("Monitoring copy already in progress.");
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(
                                "A different copy operation to destination '{0}' is already in progress.",
                                this.Destination.AbsoluteUri));
                    }
                }
                else
                {
                    if (!this.overwrite)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Destination blob {0} already exists. Not copying.",
                                this.Destination.AbsoluteUri));
                    }
                }
            }

            // Start the copy
            if (!monitorCopyInProgress)
            {
                try
                {
                    destBlob.StartCopyFromBlob(this.Source);
                }
                catch (StorageException ex)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Source Uri '{0}' is invalid.",
                            this.Source.AbsoluteUri));
                }

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

                if (destBlob.CopyState.Status == CopyStatus.Failed)
                {
                    throw new ArgumentException("Could not copy blob. The operation failed.");
                }
            }
            while (destBlob.CopyState.Status == CopyStatus.Pending && !this.Stopping);

            if (!this.Stopping)
            {
                this.WriteOperationStatus("Complete");
                this.WriteObject(new CopyAzureStorageBlobContext() { DestinationUri = this.Destination });
            }
        }

        /// <summary>
        /// Aborts the execution of the Copy-AzureStorageBlob Cmdlet.
        /// </summary>
        internal void AbortCommand()
        {
            try
            {
                if (this.destBlob != null)
                {
                    this.destBlob.FetchAttributes();
                    if (!string.IsNullOrEmpty(this.destBlob.CopyState.CopyId))
                    {
                        try
                        {
                            this.destBlob.AbortCopy(this.destBlob.CopyState.CopyId);
                        }
                        catch (StorageException ex)
                        {
                            if (ex.RequestInformation.HttpStatusCode != 409)
                            {
                                throw;
                            }
                            else
                            {
                                // The storage API always returns 409 from this call
                                // so swallow the exception so we can hit the delete call.
                            }
                        }

                        this.destBlob.DeleteIfExists();
                    }
                }
            }
            finally
            {
                base.StopProcessing();
            }
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
            catch (StorageException)
            {
                return false;
            }
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
            this.WriteProgress(new ProgressRecord(1, "Copy-AzureStorageBlob", string.Format("Operation Status: {0}", status)));
        }
    }
}
