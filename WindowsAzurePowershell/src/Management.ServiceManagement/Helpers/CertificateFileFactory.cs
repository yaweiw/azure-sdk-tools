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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using WindowsAzure.ServiceManagement;
    using Helpers;

    public class CertificateFileFactory
    {
        public static CertificateFile Create(X509Certificate2 certificate)
        {
            var certificateData = CertUtils.GetCertificateData(certificate);
            var certificateFile = new CertificateFile
                                      {
                                          Data = Convert.ToBase64String(certificateData),
                                          Password = CertUtils.RandomBase64PasswordString(),
                                          CertificateFormat = "pfx"
                                      };
            return certificateFile;
        }

        public static CertificateFile Create(X509Certificate2 certificate, bool dropPrivateKey)
        {
            if(dropPrivateKey)
            {
                certificate = CertUtils.DropPrivateKey(certificate);
            }
            var certificateData = CertUtils.GetCertificateData(certificate);
            var certificateFile = new CertificateFile
                                      {
                                          Data = Convert.ToBase64String(certificateData),
                                          Password = CertUtils.RandomBase64PasswordString(),
                                          CertificateFormat = "pfx"
                                      };
            return certificateFile;
        }
    }
}