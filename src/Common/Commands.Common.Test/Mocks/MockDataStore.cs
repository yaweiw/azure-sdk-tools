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

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Mocks
{
    public class MockDataStore : IDataStore
    {
        private Dictionary<string, string> virtualStore = new Dictionary<string, string>();

        public Dictionary<string, string> VirtualStore
        {
            get { return virtualStore; }
            set { virtualStore = value; }
        }

        public void WriteFile(string path, string contents)
        {
            VirtualStore[path] = contents;
        }

        public void WriteFile(string path, byte[] contents)
        {
            VirtualStore[path] = Encoding.Default.GetString(contents);
        }

        public string ReadFileAsText(string path)
        {
            if (VirtualStore.ContainsKey(path))
            {
                return VirtualStore[path];
            }
            else
            {
                throw new IOException("File not found: " + path);
            }
        }

        public Stream ReadFileAsStream(string path)
        {
            if (VirtualStore.ContainsKey(path))
            {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(VirtualStore[path]);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
            else
            {
                throw new IOException("File not found: " + path);
            }
        }

        public byte[] ReadFileAsBytes(string path)
        {
            if (VirtualStore.ContainsKey(path))
            {
                return Encoding.Default.GetBytes(VirtualStore[path]);
            }
            else
            {
                throw new IOException("File not found: " + path);
            }
        }

        public void RenameFile(string oldPath, string newPath)
        {
            if (VirtualStore.ContainsKey(oldPath))
            {
                VirtualStore[newPath] = VirtualStore[oldPath];
                VirtualStore.Remove(oldPath);
            }
            else
            {
                throw new IOException("File not found: " + oldPath);
            }
        }

        public bool FileExists(string path)
        {
            return VirtualStore.ContainsKey(path);
        }

        public X509Certificate2 GetCertificate(string thumbprint)
        {
            throw new System.NotImplementedException();
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            throw new System.NotImplementedException();
        }
    }
}
