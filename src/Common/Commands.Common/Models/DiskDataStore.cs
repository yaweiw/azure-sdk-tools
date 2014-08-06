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

using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public class DiskDataStore : IDataStore
    {
        private string profileDirectory;

        public DiskDataStore(string profileDirectory)
        {
            this.profileDirectory = profileDirectory;
        }

        public void WriteAllText(string file, string contents)
        {
            File.WriteAllText(Path.Combine(profileDirectory, file), contents);
        }

        public void WriteAllBytes(string file, byte[] contents)
        {
            File.WriteAllBytes(Path.Combine(profileDirectory, file), contents);
        }

        public string ReadAllText(string file)
        {
            return File.ReadAllText(Path.Combine(profileDirectory, file));
        }

        public byte[] ReadAllBytes(string file)
        {
            return File.ReadAllBytes(Path.Combine(profileDirectory, file));
        }

        public bool FileExists(string file)
        {
            return File.Exists(Path.Combine(profileDirectory, file));
        }

        public X509Certificate2 GetCertificate(string thumbprint)
        {
            return GeneralUtilities.GetCertificateFromStore(thumbprint);
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            GeneralUtilities.AddCertificateToStore(cert);
        }

        public void DeleteFile(string file)
        {
            File.Delete(Path.Combine(profileDirectory, file));
        }

        public string ProfileDirectory { get { return profileDirectory; } }
    }
}
