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
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public static class CertUtils
    {
        public static byte[] GetCertificateData(X509Certificate2 cert)
        {
            try
            {
                return cert.HasPrivateKey ? cert.Export(X509ContentType.Pfx) : cert.Export(X509ContentType.Pkcs12);
            }
            catch (CryptographicException)
            {
                return cert.HasPrivateKey ? cert.RawData : cert.Export(X509ContentType.Pkcs12);
            }
        }

        public static byte[] GetCertificateData(string certPath, string password)
        {
            var cert = new X509Certificate2();
            cert.Import(certPath, password, X509KeyStorageFlags.Exportable);
            return cert.HasPrivateKey ? cert.Export(X509ContentType.Pfx, password) : cert.Export(X509ContentType.Pkcs12);
        }

        public static X509Certificate2 DropPrivateKey(X509Certificate2 cert)
        {
            // export and reimport without private key.
            var noPrivateKey = cert.Export(X509ContentType.Cert);
            return new X509Certificate2(noPrivateKey);
        }

        #region from CsUpload

        public static string RandomBase64PasswordString()
        {
            return RandomBase64String(32);
        }

        public static string RandomBase64String(int length)
        {
            using(var rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[length];
                rng.GetBytes(data);
                return Convert.ToBase64String(data);
            }
        }

        private static string CanonicalizeThumbprintString(string s)
        {
            var chars = from ch in s.ToCharArray()
                        where !Char.IsWhiteSpace(ch)
                        select Char.ToUpperInvariant(ch);
            return new String(chars.ToArray());
        }

        private static bool IsValidThumbprint(string thumbprint)
        {
            if (String.IsNullOrEmpty(thumbprint))
            {
                return false;
            }
            var isHexString = thumbprint.ToCharArray().All(Uri.IsHexDigit);
            if (!isHexString)
            {
                return false;
            }
            return true;
        }

        public static X509Certificate2 FindCertificate(X509Certificate2[] certificates, string thumbprint)
        {
            X509Certificate2 result = null;
            result = certificates.FirstOrDefault(cert => String.Compare(cert.Thumbprint, thumbprint, StringComparison.InvariantCultureIgnoreCase) == 0);
            return result;
        }

        public static X509Certificate2 FindCertificate(string thumbprint)
        {
            var cannonicalized = CanonicalizeThumbprintString(thumbprint);
            if (!IsValidThumbprint(cannonicalized))
            {
//                Program.Output.ErrorInvalidThumbprint(thumbprint);
                return null;
            }

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cannonicalized, false);
                if (certs == null || certs.Count == 0)
                {
//                    Program.Output.ErrorCertificateNotFound(thumbprint);
                    return null;
                }

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }
        #endregion
    }
}
