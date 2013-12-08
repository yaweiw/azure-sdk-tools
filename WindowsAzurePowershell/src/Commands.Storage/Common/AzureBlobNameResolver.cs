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

namespace Microsoft.WindowsAzure.Commands.Storage.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class AzureBlobNameRequestResolver
    {
        private string root;
        private Queue<string> files;

        public BlobType Type { get; set; }
        public CloudBlobContainer Container { get; set; }
        public string BlobName { get; set; }

        public AzureBlobNameRequestResolver()
        {
            root = string.Empty;
            files = new Queue<string>();
        }

        public bool AddFile(string absolutFilePath)
        {
            if (!System.IO.File.Exists(absolutFilePath))
            {
                if (System.IO.Directory.Exists(absolutFilePath))
                {
                    return false;
                }
                else
                {
                    throw new ArgumentException(String.Format(Resources.FileNotFound, absolutFilePath));
                }
            }

            string dirPath = Path.GetDirectoryName(absolutFilePath).ToLower();

            if (string.IsNullOrEmpty(root) || !dirPath.StartsWith(root))
            {
                root = GetCommonDirectory(root, dirPath);
            }

            files.Enqueue(absolutFilePath);
            return true;
        }

        public bool IsEmpty()
        {
            return files.Count == 0;
        }

        public Tuple<string, ICloudBlob> GetFileAndBlobTuple()
        {
            string filePath = files.Dequeue();
            string blobName = string.Empty;

            if (!string.IsNullOrEmpty(BlobName))
            {
                blobName = BlobName;
            }
            else
            {
                blobName = filePath.Substring(root.Length);
            }

            ICloudBlob blob = default(ICloudBlob);

            switch (Type)
            {
                case BlobType.PageBlob:
                    blob = Container.GetPageBlobReference(blobName);
                    break;
                case BlobType.BlockBlob:
                default:
                    blob = Container.GetBlockBlobReference(blobName);
                    break;
            }

            return new Tuple<string, ICloudBlob>(filePath, blob);
        }

        private string GetCommonDirectory(string dir1, string dir2)
        {
            if (string.IsNullOrEmpty(dir1) || string.IsNullOrEmpty(dir2))
            { 
                return string.IsNullOrEmpty(dir2) ? dir1 : dir2;
            }

            string [] path1 = dir1.Split(Path.DirectorySeparatorChar);
            string[] path2 = dir2.Split(Path.DirectorySeparatorChar);
            return GetCommonDirectory(path1, path2);
        }

        private string GetCommonDirectory(string[] path1, string[] path2)
        {
            if (path1.Length > path2.Length)
            {
                return GetCommonDirectory(path2, path1);
            }

            string prefix = string.Empty;

            for (int i = 0; i < path1.Length; i++)
            {
                if (path1[i] == path2[i])
                {
                    prefix += path1[i] + Path.DirectorySeparatorChar;
                }
                else
                {
                    break;
                }
            }

            return prefix;
        }
    }
}
