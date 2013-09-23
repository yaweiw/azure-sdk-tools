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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// This class encapsulates the details of creating and
    /// reloading certificates from various sources.
    /// </summary>
    public class WindowsAzureCertificate
    {
        private readonly X509Certificate2 certificate;
        private readonly string managementCertificateString;

        public X509Certificate2 Certificate { get { return certificate; } }
        public string CertificateString { get { return managementCertificateString; } }

        public WindowsAzureCertificate(string managementCertificateString)
        {
            this.managementCertificateString = managementCertificateString;
            certificate = new X509Certificate2(Convert.FromBase64String(managementCertificateString), string.Empty);
        }

        /// <summary>
        /// Implicit conversion operator to X509Certificate2 type
        /// so that you can use this class anywhere you use
        /// X509Certificate2.
        /// </summary>
        /// <param name="cert">Certificate to convert</param>
        /// <returns>The x509Certificate2</returns>
        public static implicit operator X509Certificate2(WindowsAzureCertificate cert)
        {
            return cert.certificate;
        }
    }
}
