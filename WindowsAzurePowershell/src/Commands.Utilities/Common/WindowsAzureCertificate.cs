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
    using System.Security.Permissions;
    using Properties;

    /// <summary>
    /// This class encapsulates the details of creating and
    /// reloading certificates from various sources.
    /// </summary>
    public class WindowsAzureCertificate
    {
        private readonly X509Certificate2 certificate;

        public X509Certificate2 Certificate { get { return certificate; } }
        public string Thumbprint { get { return certificate.Thumbprint; } }

        public WindowsAzureCertificate(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public static WindowsAzureCertificate FromPublishSettingsString(string managementCertificateString)
        {
            var certificate = new X509Certificate2(Convert.FromBase64String(managementCertificateString), string.Empty);
            SaveCertificateToStore(certificate);
            return new WindowsAzureCertificate(certificate);
        }

        public static WindowsAzureCertificate FromThumbprint(string thumbprint)
        {
            var certificate = LoadCertificateByThumbprint(thumbprint);
            return new WindowsAzureCertificate(certificate);
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

        [StorePermission(SecurityAction.Demand, Unrestricted = true)]
        private static void SaveCertificateToStore(X509Certificate2 certificate)
        {
            DoStoreOp(OpenFlags.ReadWrite, store => store.Add(certificate));
        }

        [StorePermission(SecurityAction.Demand, Flags = StorePermissionFlags.OpenStore | StorePermissionFlags.EnumerateCertificates)]
        private static X509Certificate2 LoadCertificateByThumbprint(string thumbprint)
        {
            X509Certificate2 certificate = null;
            Action<X509Store> findCert = store => {
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certs.Count > 0)
                {
                    certificate = certs[0];
                }
            };

            DoStoreOp(StoreLocation.CurrentUser, OpenFlags.ReadOnly, findCert);
            if (certificate == null)
            {
                DoStoreOp(StoreLocation.LocalMachine, OpenFlags.ReadOnly, findCert);
            }

            if (certificate == null)
            {
                throw new ArgumentException(string.Format(Resources.CertificateNotFoundInStore, thumbprint));
            }

            return certificate;
        }

        private static void DoStoreOp(OpenFlags flags, Action<X509Store> op)
        {
            DoStoreOp(StoreLocation.CurrentUser, flags, op);
        }

        private static void DoStoreOp(StoreLocation location, OpenFlags flags, Action<X509Store> op)
        {
            var store = new X509Store(StoreName.My, location);
            using (new StoreOpener(store, flags))
            {
                op(store);
            }
        }



        // Helper class to manage opening a closing a certificate store
        private sealed class StoreOpener : IDisposable
        {
            private X509Store store;

            public StoreOpener(X509Store store, OpenFlags openFlags)
            {
                store.Open(openFlags);
                this.store = store;
            }

            public void Dispose()
            {
                if (store != null)
                {
                    store.Close();
                    store = null;
                }
            }
        }
    }
}
