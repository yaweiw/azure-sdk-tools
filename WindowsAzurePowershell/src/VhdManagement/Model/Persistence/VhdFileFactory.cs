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

namespace Microsoft.WindowsAzure.Management.Tools.Vhd.Model.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Tools.Common.General;

    public class VhdFileFactory
    {
        public VhdFile Create(string path)
        {           
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return Create(fileStream, Path.GetDirectoryName(path));
        }

        public VhdFile Create(Stream stream)
        {
            return Create(stream, null);
        }

        private VhdFile Create(Stream stream, string vhdDirectory)
        {
            var reader = new BinaryReader(stream, Encoding.Unicode);
            try
            {
                var dataReader = new VhdDataReader(reader);
                var footer = new VhdFooterFactory(dataReader).CreateFooter();

                VhdHeader header = null;
                BlockAllocationTable blockAllocationTable = null;
                VhdFile parent = null;
                if (footer.DiskType != DiskType.Fixed)
                {
                    header = new VhdHeaderFactory(dataReader, footer).CreateHeader();
                    blockAllocationTable = new BlockAllocationTableFactory(dataReader, header).Create();
                    if (footer.DiskType == DiskType.Differencing)
                    {
                        var parentPath = vhdDirectory == null ? header.ParentPath : Path.Combine(vhdDirectory, header.GetRelativeParentPath());
                        parent = Create(parentPath);
                    }
                }
                return new VhdFile(footer, header, blockAllocationTable, parent, stream);
            }
            catch (EndOfStreamException)
            {
                throw new VhdParsingException("unsupported format");
            }
        }

        private T TryCatch<T>(Func<IAsyncResult, T> method, IAsyncResult result)
        {
            try
            {
                return method(result);
            }
            catch (EndOfStreamException e)
            {
                throw new VhdParsingException("unsupported format", e);
            }
        }

        public IAsyncResult BeginCreate(string path, AsyncCallback callback, object state)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            return BeginCreate(stream, Path.GetDirectoryName(path), callback, state);
        }

        public IAsyncResult BeginCreate(Stream stream, AsyncCallback callback, object state)
        {
            return BeginCreate(stream, null, callback, state);
        }

        private IAsyncResult BeginCreate(Stream stream, string vhdDirectory, AsyncCallback callback, object state)
        {
            var streamSource = new StreamSource {Stream = stream, VhdDirectory = vhdDirectory};
            return AsyncMachine<VhdFile>.BeginAsyncMachine(this.CreateAsync, streamSource, callback, state);
        }

        public VhdFile EndCreate(IAsyncResult result)
        {
            return AsyncMachine<VhdFile>.EndAsyncMachine(result);
        }

        class StreamSource
        {
            public Stream Stream { get; set; }
            public string VhdDirectory { get; set; }
        }

        private IEnumerable<CompletionPort> CreateAsync(AsyncMachine<VhdFile> machine, StreamSource streamSource)
        {
            var reader = new BinaryReader(streamSource.Stream, Encoding.Unicode);
            var dataReader = new VhdDataReader(reader);
            var footerFactory = new VhdFooterFactory(dataReader);

            footerFactory.BeginCreateFooter(machine.CompletionCallback, null);
            yield return CompletionPort.SingleOperation;
            var footer = TryCatch<VhdFooter>(footerFactory.EndCreateFooter, machine.CompletionResult);

            VhdHeader header = null;
            BlockAllocationTable blockAllocationTable = null;
            VhdFile parent = null;
            if (footer.DiskType != DiskType.Fixed)
            {
                var headerFactory = new VhdHeaderFactory(dataReader, footer);

                headerFactory.BeginCreateHeader(machine.CompletionCallback, null);
                yield return CompletionPort.SingleOperation;
                header = TryCatch<VhdHeader>(headerFactory.EndCreateHeader, machine.CompletionResult);

                var tableFactory = new BlockAllocationTableFactory(dataReader, header);
                tableFactory.BeginCreate(machine.CompletionCallback, null);
                yield return CompletionPort.SingleOperation;
                blockAllocationTable = TryCatch<BlockAllocationTable>(tableFactory.EndCreate, machine.CompletionResult);

                if (footer.DiskType == DiskType.Differencing)
                {
                    var parentPath = streamSource.VhdDirectory == null ? header.ParentPath : Path.Combine(streamSource.VhdDirectory, header.GetRelativeParentPath());

                    BeginCreate(parentPath, machine.CompletionCallback, null);
                    yield return CompletionPort.SingleOperation;
                    parent = EndCreate(machine.CompletionResult);
                }
            }
            machine.ParameterValue =  new VhdFile(footer, header, blockAllocationTable, parent, streamSource.Stream);
        }
    }
}