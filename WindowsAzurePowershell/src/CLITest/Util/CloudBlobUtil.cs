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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MS.Test.Common.MsTestLib;
using StorageTestLib;
using Storage = Microsoft.WindowsAzure.Storage.Blob;

namespace CLITest.Util
{
    public class CloudBlobUtil
    {
        private CloudStorageAccount account;
        private CloudBlobClient client;
        private Random random;
        private const int PageBlobUnitSize = 512;

        private CloudBlobUtil()
        { }

        /// <summary>
        /// init cloud blob util
        /// </summary>
        /// <param name="account">storage account</param>
        public CloudBlobUtil(CloudStorageAccount account)
        {
            this.account = account;
            client = account.CreateCloudBlobClient();
            random = new Random();
        }

        /// <summary>
        /// create a container with random properties and metadata
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <returns>the created container object with properties and metadata</returns>
        public CloudBlobContainer CreateContainer(string containerName)
        {
            CloudBlobContainer container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            //there is no properties to set
            container.FetchAttributes();

            int minMetaCount = 1;
            int maxMetaCount = 5;
            int minMetaValueLength = 10;
            int maxMetaValueLength = 20;
            int count = random.Next(minMetaCount, maxMetaCount);
            for (int i = 0; i < count; i++)
            {
                string metaKey = Utility.GenNameString("metatest");
                int valueLength = random.Next(minMetaValueLength, maxMetaValueLength);
                string metaValue = Utility.GenNameString("metavalue-", valueLength);
                container.Metadata.Add(metaKey, metaValue);
            }

            container.SetMetadata();

            Test.Info(string.Format("create container '{0}'", containerName));
            return container;
        }

        public CloudBlobContainer CreateContainer(string containerName, BlobContainerPublicAccessType permission)
        {
            CloudBlobContainer container = CreateContainer(containerName);
            BlobContainerPermissions containerPermission = new BlobContainerPermissions();
            containerPermission.PublicAccess = permission;
            container.SetPermissions(containerPermission);
            return container;
        }

        /// <summary>
        /// create mutiple containers
        /// </summary>
        /// <param name="containerNames">container names list</param>
        /// <returns>a list of container object</returns>
        public List<CloudBlobContainer> CreateContainer(List<string> containerNames)
        {
            List<CloudBlobContainer> containers = new List<CloudBlobContainer>();

            foreach (string name in containerNames)
            {
                containers.Add(CreateContainer(name));
            }

            containers = containers.OrderBy(container => container.Name).ToList();

            return containers;
        }

        /// <summary>
        /// remove specified container
        /// </summary>
        /// <param name="containerName">container name</param>
        public void RemoveContainer(string containerName)
        {
            CloudBlobContainer container = client.GetContainerReference(containerName);
            container.DeleteIfExists();
            Test.Info(string.Format("remove container '{0}'", containerName));
        }

        /// <summary>
        /// remove a list containers
        /// </summary>
        /// <param name="containerNames">container names</param>
        public void RemoveContainer(List<string> containerNames)
        {
            foreach (string name in containerNames)
            {
                try
                {
                    RemoveContainer(name);
                }
                catch (Exception e)
                {
                    Test.Warn(string.Format("Can't remove container {0}. Exception: {1}", name, e.Message));
                }
            }
        }

        /// <summary>
        /// create a new page blob with random properties and metadata
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">blob name</param>
        /// <returns>ICloudBlob object</returns>
        public ICloudBlob CreatePageBlob(CloudBlobContainer container, string blobName)
        {
            CloudPageBlob pageBlob = container.GetPageBlobReference(blobName);
            int size = random.Next(1, 10) * PageBlobUnitSize;
            pageBlob.Create(size);
            byte[] buffer = new byte[size];
            string md5sum = Convert.ToBase64String(Helper.GetMD5(buffer));
            pageBlob.Properties.ContentMD5 = md5sum;
            GenerateBlobPropertiesAndMetaData(pageBlob);
            Test.Info(string.Format("create page blob '{0}' in container '{1}'", blobName, container.Name));
            return pageBlob;
        }

        /// <summary>
        /// create a block blob with random properties and metadata
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">Block blob name</param>
        /// <returns>ICloudBlob object</returns>
        public ICloudBlob CreateBlockBlob(CloudBlobContainer container, string blobName)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            
            int maxBlobSize = 1024 * 1024;
            string md5sum = string.Empty;
            int blobSize = random.Next(maxBlobSize);
            byte[] buffer = new byte[blobSize];
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                random.NextBytes(buffer);
                //ms.Read(buffer, 0, buffer.Length);
                blockBlob.UploadFromStream(ms);
                md5sum = Convert.ToBase64String(Helper.GetMD5(buffer));
            }

            blockBlob.Properties.ContentMD5 = md5sum;
            GenerateBlobPropertiesAndMetaData(blockBlob);
            Test.Info(string.Format("create block blob '{0}' in container '{1}'", blobName, container.Name));
            return blockBlob;
        }

        /// <summary>
        /// generate random blob properties and metadata
        /// </summary>
        /// <param name="blob">ICloudBlob object</param>
        private void GenerateBlobPropertiesAndMetaData(ICloudBlob blob)
        {
            blob.Properties.ContentEncoding = Utility.GenNameString("encoding");
            blob.Properties.ContentLanguage = Utility.GenNameString("lang");

            int minMetaCount = 1;
            int maxMetaCount = 5;
            int minMetaValueLength = 10;
            int maxMetaValueLength = 20;
            int count = random.Next(minMetaCount, maxMetaCount);

            for (int i = 0; i < count; i++)
            {
                string metaKey = Utility.GenNameString("metatest");
                int valueLength = random.Next(minMetaValueLength, maxMetaValueLength);
                string metaValue = Utility.GenNameString("metavalue-", valueLength);
                blob.Metadata.Add(metaKey, metaValue);
            }

            blob.SetProperties();
            blob.SetMetadata();
            blob.FetchAttributes();
        }

        /// <summary>
        /// Create a blob with specified blob type
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">Blob name</param>
        /// <param name="type">Blob type</param>
        /// <returns>ICloudBlob object</returns>
        public ICloudBlob CreateBlob(CloudBlobContainer container, string blobName, Storage.BlobType type)
        {
            if (type == Microsoft.WindowsAzure.Storage.Blob.BlobType.BlockBlob)
            {
                return CreateBlockBlob(container, blobName);
            }
            else
            {
                return CreatePageBlob(container, blobName);
            }
        }

        /// <summary>
        /// create a list of blobs with random properties/metadata/blob type
        /// </summary>
        /// <param name="container">CloudBlobContainer object</param>
        /// <param name="blobName">a list of blob names</param>
        /// <returns>a list of cloud page blobs</returns>
        public List<ICloudBlob> CreateRandomBlob(CloudBlobContainer container, List<string> blobNames)
        {
            List<ICloudBlob> blobs = new List<ICloudBlob>();

            int switchKey = 0;

            foreach (string blobName in blobNames)
            {
                switchKey = random.Next(2);

                if (switchKey == 0)
                {
                    blobs.Add(CreatePageBlob(container, blobName));
                }
                else
                { 
                    blobs.Add(CreateBlockBlob(container, blobName));
                }
            }

            blobs = blobs.OrderBy(blob => blob.Name).ToList();

            return blobs;
        }

        /// <summary>
        /// convert blob name into valid file name
        /// </summary>
        /// <param name="blobName">blob name</param>
        /// <returns>valid file name</returns>
        public string ConvertBlobNameToFileName(string blobName, string dir, DateTimeOffset? snapshotTime = null)
        {
            string fileName = blobName;

            //replace dirctionary
            Dictionary<string, string> replaceRules = new Dictionary<string, string>()
	            {
	                {"/", "\\"}
	            };

            foreach (KeyValuePair<string, string> rule in replaceRules)
            {
                fileName = fileName.Replace(rule.Key, rule.Value);
            }

            if (snapshotTime != null)
            {
                int index = fileName.LastIndexOf('.');

                string prefix = string.Empty;
                string postfix = string.Empty;
                string timeStamp = string.Format("{0:u}", snapshotTime.Value);
                timeStamp = timeStamp.Replace(":", string.Empty).TrimEnd(new char[] { 'Z' });

                if (index == -1)
                {
                    prefix = fileName;
                    postfix = string.Empty;
                }
                else
                {
                    prefix = fileName.Substring(0, index);
                    postfix = fileName.Substring(index);
                }

                fileName = string.Format("{0} ({1}){2}", prefix, timeStamp, postfix);
            }

            return Path.Combine(dir, fileName);
        }

        public string ConvertFileNameToBlobName(string fileName)
        {
            return fileName.Replace('\\', '/');
        }

        /// <summary>
        /// list all the existing containers
        /// </summary>
        /// <returns>a list of cloudblobcontainer object</returns>
        public List<CloudBlobContainer> GetExistingContainers()
        {
            ContainerListingDetails details = ContainerListingDetails.All;
            return client.ListContainers(string.Empty, details).ToList();
        }

        /// <summary>
        /// get the number of existing container
        /// </summary>
        /// <returns></returns>
        public int GetExistingContainerCount()
        {
            return GetExistingContainers().Count;
        }

        public static void PackContainerCompareData(CloudBlobContainer container, Dictionary<string, object> dic)
        {
            BlobContainerPermissions permissions = container.GetPermissions();
            dic["PublicAccess"] = permissions.PublicAccess;
            dic["Permission"] = permissions;
            dic["LastModified"] = container.Properties.LastModified;
        }

        public static void PackBlobCompareData(ICloudBlob blob, Dictionary<string, object> dic)
        {
            dic["Length"] = blob.Properties.Length;
            dic["ContentType"] = blob.Properties.ContentType;
            dic["LastModified"] = blob.Properties.LastModified;
            dic["SnapshotTime"] = blob.SnapshotTime;
        }
    }
}
