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

namespace Microsoft.WindowsAzure.Management.Sync.Download
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Channels;
    using Sync.Threading;
    using Tools.Vhd.Model;

    public class Downloader
    {
        string StorageAccountKey;

        private BlobUri blobUri;

        public Downloader(BlobUri blobUri, string storageAccountKey)
        {
            this.blobUri = blobUri;
            this.StorageAccountKey = storageAccountKey;
        }

        public void Download(string destination)
        {
            DeleteTempVhdIfExist(destination);

            Console.WriteLine("\t\tDownloading blob '{0}' ...", blobUri.BlobName);
            Console.WriteLine("\t\tImage download start time: '{0}'", DateTime.UtcNow.ToString("o"));

            var blobHandle = new BlobHandle(blobUri, this.StorageAccountKey);

            const int megaByte = 1024 * 1024;

            var ranges = blobHandle.GetUploadableRanges();
            var bufferManager = BufferManager.CreateBufferManager(Int32.MaxValue, 20 * megaByte);
            var downloadStatus = new ProgressStatus(0, ranges.Sum(r => r.Length), new ComputeStats());

            Trace.WriteLine(String.Format("Total Data:{0}", ranges.Sum(r => r.Length)));

            const int maxParallelism = 24;
            using (new ServicePointHandler(this.blobUri.Uri, maxParallelism))
            using (new ProgressTracker(downloadStatus, Program.SyncOutput.ProgressUploadStatus, Program.SyncOutput.ProgressUploadComplete, TimeSpan.FromSeconds(1)))
            {
//                if(SparseFile.VolumeSupportsSparseFiles(destination))
//                {
//                   using(var fileStream = SparseFile.Create(destination))
//                   {
//                       foreach (var emptyRange in blobHandle.GetEmptyRanges())
//                       {
//                           SparseFile.SetSparseRange(fileStream.SafeFileHandle, emptyRange.StartIndex, emptyRange.Length);
//                       }
//                   }
//                }

                using (var fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 8 * megaByte, FileOptions.WriteThrough))
                {
                    fileStream.SetLength(0);
                    fileStream.SetLength(blobHandle.Length);

                    LoopResult lr = Parallel.ForEach<IndexRange, Stream>(ranges,
                                     blobHandle.OpenStream,
                                     (r, b) =>
                                     {
                                         b.Seek(r.StartIndex, SeekOrigin.Begin);

                                         byte[] buffer = this.EnsureReadAsSize(b, (int)r.Length, bufferManager);

                                         lock (fileStream)
                                         {
                                             Trace.WriteLine(String.Format("Range:{0}", r));
                                             fileStream.Seek(r.StartIndex, SeekOrigin.Begin);
                                             fileStream.Write(buffer, 0, (int)r.Length);
                                             fileStream.Flush();
                                         }

                                         downloadStatus.AddToProcessedBytes((int)r.Length);
                                     },
                                     pbwlf =>
                                     {
                                         pbwlf.Dispose();
                                     },
                                     maxParallelism);
                    if (lr.IsExceptional)
                    {
                        Console.WriteLine("\t\tException(s) happened");
                        for (int i = 0; i < lr.Exceptions.Count; i++)
                        {
                            Console.WriteLine("{0} -> {1}", i, lr.Exceptions[i]);
                        }
                    }
                }
            }

            Console.WriteLine("\t\tImage download end time  : '{0}'", DateTime.UtcNow.ToString("o"));
        }

        private void DeleteTempVhdIfExist(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        private byte[] EnsureReadAsSize(Stream stream, int size, BufferManager manager)
        {
            byte[] buffer = manager.TakeBuffer(size);
            int byteRead = 0;
            int totalRead = 0;
            int sizeLeft = size;
            do
            {
                byteRead = stream.Read(buffer, totalRead, sizeLeft);
                totalRead += byteRead;
                if (totalRead == size)
                {
                    break;
                }

                sizeLeft = sizeLeft - byteRead;
            } while (true);

            return buffer;
        }
    }
}
