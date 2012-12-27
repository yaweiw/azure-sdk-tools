// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// base cmdlet for storage blob/container cmdlet
    /// </summary>
    public class StorageBlobBaseCmdlet : StorageBaseCmdlet
    {
        internal IBlobManagement blobClient { get; set; }
        
        //auto clean blob client in order to work with multiple storage account
        private bool autoClean = false;

        /// <summary>
        /// Make sure the pipeline blob is valid and already existing
        /// </summary>
        /// <param name="blob">ICloudBlob object</param>
        internal void ValidatePipelineICloudBlob(ICloudBlob blob)
        {
            if (null == blob)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            }
            if (!NameUtil.IsValidBlobName(blob.Name))
            {
                throw new ArgumentException(String.Format(Resources.InValidBlobName, blob.Name));
            }
            ValidatePipelineCloudBlobContainer(blob.Container);
            BlobRequestOptions requestOptions = null;
            if (!blobClient.IsBlobExists(blob, requestOptions, operationContext))
            {
                throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blob.Name, blob.Container.Name));
            }
        }

        /// <summary>
        /// Make sure the container is valid and already existing 
        /// </summary>
        /// <param name="container"></param>
        /// //TODO cache for validation? too many remote calls
        internal void ValidatePipelineCloudBlobContainer(CloudBlobContainer container)
        {
            if (null == container)
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, typeof(CloudBlobContainer).Name));
            }
            if (!NameUtil.IsValidContainerName(container.Name))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, container.Name));
            }
            BlobRequestOptions requestOptions = null;
            if (!blobClient.IsContainerExists(container, requestOptions, operationContext))
            {
                throw new ResourceNotFoundException(String.Format(Resources.ContainerNotFound, container.Name));
            }
        }

        /// <summary>
        /// get blob client
        /// </summary>
        /// <returns></returns>
        protected CloudBlobClient GetCloudBlobClient()
        {
            //use the default retry policy in storage client
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudBlobClient();
        }

        /// <summary>
        /// process record
        /// </summary>
        protected override void ProcessRecord()
        {
            if (blobClient == null)
            {
                autoClean = true;
                blobClient = new BlobManagement(GetCloudBlobClient());
            }

            try
            {
                base.ProcessRecord();
            }
            finally
            {

                if (autoClean)
                {
                    blobClient = null;
                    autoClean = false;
                }
            }
        }
    }
}