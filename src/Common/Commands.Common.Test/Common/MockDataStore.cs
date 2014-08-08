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

using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    public class MockDataStore : IDataStore
    {
        private const string profileFileName = "profile";

        private const string tokenFileName = "token";

        private Dictionary<string, object> mockStore = new Dictionary<string, object>();

        public void WriteProfile(string contents)
        {
            mockStore[profileFileName] = contents;
        }

        public void WriteTokenCache(byte[] contents)
        {
            mockStore[tokenFileName] = contents;
        }

        public string ReadProfile()
        {
            if (mockStore.ContainsKey(profileFileName))
            {
                return mockStore[profileFileName] as string;
            }
            else
            {
                return null;
            }

        }

        public byte[] ReadTokenCache()
        {
            if (mockStore.ContainsKey(tokenFileName))
            {
                return mockStore[tokenFileName] as byte[];
            }
            else
            {
                return null;
            }
        }

        public X509Certificate2 GetCertificate(string thumbprint)
        {
            throw new System.NotImplementedException();
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            throw new System.NotImplementedException();
        }

        public string ProfilePath { get { return null; } }
    }
}
