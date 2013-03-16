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

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;

    public static class General
    {
        private static Assembly _assembly = Assembly.GetExecutingAssembly();

        private static bool TryFindCertificatesInStore(string thumbprint,
            System.Security.Cryptography.X509Certificates.StoreLocation location, out X509Certificate2Collection certificates)
        {
            X509Store store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);
            certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            store.Close();

            return certificates != null && certificates.Count > 0;
        }

        private static string TryGetEnvironmentVariable(string environmentVariableName, string defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(environmentVariableName);
            return (string.IsNullOrEmpty(value)) ? defaultValue : value;
        }

        public static string GetAssemblyDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetWithProgramFilesPath(string directoryName, bool throwIfNotFound)
        {
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (Directory.Exists(Path.Combine(programFilesPath, directoryName)))
            {
                return Path.Combine(programFilesPath, directoryName);
            }
            else
            {
                if (programFilesPath.IndexOf(Resources.x86InProgramFiles) == -1)
                {
                    programFilesPath += Resources.x86InProgramFiles;
                    if (throwIfNotFound)
                    {
                        Validate.ValidateDirectoryExists(Path.Combine(programFilesPath, directoryName));
                    }
                    return Path.Combine(programFilesPath, directoryName);
                }
                else
                {
                    programFilesPath = programFilesPath.Replace(Resources.x86InProgramFiles, string.Empty);
                    if (throwIfNotFound)
                    {
                        Validate.ValidateDirectoryExists(Path.Combine(programFilesPath, directoryName));
                    }
                    return Path.Combine(programFilesPath, directoryName);
                }
            }
        }

        public static T DeserializeXmlFile<T>(string fileName, string exceptionMessage = null)
        {
            // TODO: fix and uncomment. second parameter is wrong
            // Validate.ValidateFileFull(fileName, string.Format(Resources.PathDoesNotExistForElement, string.Empty, fileName));

            T item = default(T);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream s = new FileStream(fileName, FileMode.Open))
            {
                try { item = (T)xmlSerializer.Deserialize(s); }
                catch
                {
                    if (!string.IsNullOrEmpty(exceptionMessage))
                    {
                        throw new InvalidOperationException(exceptionMessage);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return item;
        }

        public static void SerializeXmlFile<T>(T obj, string fileName)
        {
            Validate.ValidatePathName(fileName, string.Format(Resources.PathDoesNotExistForElement, string.Empty, fileName));
            Validate.ValidateStringIsNullOrEmpty(fileName, string.Empty);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                xmlSerializer.Serialize(stream, obj);
            }
        }

        public static T DeserializeXmlStream<T>(Stream stream)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T obj = (T)xmlSerializer.Deserialize(stream);
            stream.Close();

            return obj;
        }

        public static byte[] GetResourceContents(string resourceName)
        {
            Stream stream = _assembly.GetManifestResourceStream(resourceName);
            byte[] contents = new byte[stream.Length];
            stream.Read(contents, (int)stream.Position, (int)stream.Length);
            return contents;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public static void LaunchWebPage(string target)
        {
            ProcessHelper.Start(target);
        }

        public static void CreateDirectories(IEnumerable<string> directories)
        {
            foreach (string directoryName in directories)
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public static void CopyFilesFromResources(IDictionary<string, string> filesPair)
        {
            foreach (KeyValuePair<string, string> filePair in filesPair)
            {
                File.WriteAllBytes(filePair.Value, General.GetResourceContents(filePair.Key));
            }
        }

        /// <summary>
        /// Write all of the given bytes to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="mode">Mode to open the file.</param>
        /// <param name="bytes">Contents of the file.</param>
        public static void WriteAllBytes(string path, FileMode mode, byte[] bytes)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(Enum.IsDefined(typeof(FileMode), mode));
            Debug.Assert(bytes != null && bytes.Length > 0);

            // Note: We're not wrapping the file in a using statement because
            // that could lead to a double dispose when the writer is disposed.
            FileStream file = null;
            try
            {
                file = new FileStream(path, mode);
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    // Clear the reference to file so it won't get disposed twice
                    file = null;

                    writer.Write(bytes);
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Dispose();
                }
            }
        }

        public static int GetRandomFromTwo(int first, int second)
        {
            return (new Random(DateTime.Now.Millisecond).Next(2) == 0) ? first : second;
        }

        public static string[] GetResourceNames(string resourcesFullFolderName)
        {
            return _assembly.GetManifestResourceNames().Where<string>(item => item.StartsWith(resourcesFullFolderName)).ToArray<string>();
        }

        public static TResult InvokeMethod<T, TResult>(string methodName, object instance, params object[] arguments)
        {
            MethodInfo info = typeof(T).GetMethod(methodName);
            return (TResult)info.Invoke(instance, arguments);
        }

        public static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            Validate.ValidateStringIsNullOrEmpty(thumbprint, "certificate thumbprint");
            X509Certificate2Collection certificates;
            if (TryFindCertificatesInStore(thumbprint, StoreLocation.CurrentUser, out certificates) ||
                TryFindCertificatesInStore(thumbprint, StoreLocation.LocalMachine, out certificates))
            {
                return certificates[0];
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.CertificateNotFoundInStore, thumbprint));
            }
        }

        public static void AddCertificateToStore(X509Certificate2 certificate)
        {
            Validate.ValidateNullArgument(certificate, Resources.InvalidCertificate);
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
        }

        public static void RemoveCertificateFromStore(X509Certificate2 certificate)
        {
            Validate.ValidateNullArgument(certificate, Resources.InvalidCertificate);
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(certificate);
            store.Close();
        }

        public static string BuildConnectionString(
            string defaultEndpointsProtocol,
            string accountName,
            string accountKey,
            string blobEndpoint,
            string tableEndpoint,
            string queueEndpoint)
        {
            var connectionString = new StringBuilder();

            connectionString.AppendFormat(
                CultureInfo.InvariantCulture,
                "DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2}",
                defaultEndpointsProtocol ?? "http",
                accountName,
                accountKey);

            if (!string.IsNullOrEmpty(blobEndpoint))
            {
                connectionString.AppendFormat(CultureInfo.InvariantCulture, ";BlobEndpoint={0}", blobEndpoint);
            }

            if (!string.IsNullOrEmpty(tableEndpoint))
            {
                connectionString.AppendFormat(CultureInfo.InvariantCulture, ";TableEndpoint={0}", tableEndpoint);
            }

            if (!string.IsNullOrEmpty(queueEndpoint))
            {
                connectionString.AppendFormat(CultureInfo.InvariantCulture, ";QueueEndpoint={0}", queueEndpoint);
            }

            return connectionString.ToString();
        }

        /// <summary>
        /// Gets the value of publish settings url from environment if set, otherwise returns the default value.
        /// </summary>
        public static string PublishSettingsUrl
        {
            get
            {
                return TryGetEnvironmentVariable(Resources.PublishSettingsUrlEnv, Resources.PublishSettingsUrl);
            }
        }

        /// <summary>
        /// Gets the value of azure portal url from environment if set, otherwise returns the default value.
        /// </summary>
        public static string AzurePortalUrl
        {
            get
            {
                return TryGetEnvironmentVariable(Resources.AzurePortalUrlEnv, Resources.AzurePortalUrl);
            }
        }

        /// <summary>
        /// Gets the value of azure host name suffix from environment if set, otherwise returns the default value.
        /// </summary>
        public static string AzureWebsiteHostNameSuffix
        {
            get
            {
                return TryGetEnvironmentVariable(Resources.AzureHostNameSuffixEnv, Resources.AzureHostNameSuffix);
            }
        }

        /// <summary>
        /// Gets the value of publish settings url with realm from environment if set, otherwise returns the default value.
        /// </summary>
        /// <param name="realm">Realm phrase</param>
        /// <returns>The publish settings url with realm phrase</returns>
        public static string PublishSettingsUrlWithRealm(string realm)
        {
            return PublishSettingsUrl + "&whr=" + realm;
        }

        /// <summary>
        /// Gets the value of blob endpoint uri from environment if set, otherwise returns the default value.
        /// </summary>
        /// <param name="accountName">The storage account name</param>
        /// <returns>The full blob endpoint uri including the storage account name</returns>
        public static string BlobEndpointUri(string accountName)
        {
            return string.Format(CultureInfo.InvariantCulture,
                TryGetEnvironmentVariable(Resources.BlobEndpointUriEnv, Resources.BlobEndpointUri),
                accountName);
        }

        /// <summary>
        /// Launches windows azure management portal with specific service if specified.
        /// </summary>
        /// <param name="serviceUrl">The service uri.</param>
        /// <param name="Realm">Realm of the account.</param>
        public static void LaunchWindowsAzurePortal(string serviceUrl, string Realm)
        {
            Validate.ValidateInternetConnection();

            UriBuilder uriBuilder = new UriBuilder(General.AzurePortalUrl);
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                uriBuilder.Fragment += serviceUrl;
            }

            if (Realm != null)
            {
                string queryToAppend = string.Format("whr={0}", Realm);
                if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                {
                    uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + queryToAppend;
                }
                else
                {
                    uriBuilder.Query = queryToAppend;
                }
            }

            General.LaunchWebPage(uriBuilder.ToString());
        }

        /// <summary>
        /// Convert the given array into string.
        /// </summary>
        /// <typeparam name="T">The type of the object array is holding</typeparam>
        /// <param name="array">The collection</param>
        /// <param name="delimiter">The used delimiter between array elements</param>
        /// <returns>The array into string representation</returns>
        public static string ArrayToString<T>(this T[] array, string delimiter)
        {
            return (array == null) ? null : (array.Length == 0) ? string.Empty : array.Skip(1).Aggregate(new StringBuilder(array[0].ToString()), (s, i) => s.Append(delimiter).Append(i), s => s.ToString());
        }

        /// <summary>
        /// Copies a directory.
        /// </summary>
        /// <param name="sourceDirName">The source directory name</param>
        /// <param name="destDirName">The destination directory name</param>
        /// <param name="copySubDirs">Should the copy be recursive</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.PathDoesNotExist, sourceDirName));
            }

            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Compares two strings with handling special case that base string can be empty.
        /// </summary>
        /// <param name="leftHandSide">The base string.</param>
        /// <param name="rightHandSide">The comparer string.</param>
        /// <returns>True if equals or leftHandSide is null/empty, false otherwise.</returns>
        public static bool TryEquals(string leftHandSide, string rightHandSide)
        {
            if (string.IsNullOrEmpty(leftHandSide) ||
                leftHandSide.Equals(rightHandSide, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static string ReadBody(ref Message originalMessage)
        {
            StringBuilder strBuilder = new StringBuilder();

            using (MessageBuffer messageBuffer = originalMessage.CreateBufferedCopy(int.MaxValue))
            {
                Message message = messageBuffer.CreateMessage();
                XmlWriter writer = XmlWriter.Create(strBuilder);
                using (XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer))
                {
                    message.WriteBodyContents(dictionaryWriter);
                }

                originalMessage = messageBuffer.CreateMessage();
            }

            return Beautify(strBuilder.ToString());
        }

        /// <summary>
        /// Formats given string into well formatted XML.
        /// </summary>
        /// <param name="unformattedXml">The unformatted xml string</param>
        /// <returns>The formatted XML string</returns>
        public static string Beautify(string unformattedXml)
        {
            string formattedXml = string.Empty;
            if (!string.IsNullOrEmpty(unformattedXml))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(unformattedXml);
                StringBuilder stringBuilder = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(stringBuilder, settings))
                {
                    doc.Save(writer);
                }
                formattedXml = stringBuilder.ToString();
            }

            return formattedXml;
        }

        public static string GetConfiguration(string configurationPath)
        {
            var configuration = string.Join(string.Empty, File.ReadAllLines(configurationPath));
            return ServiceManagementHelper.EncodeToBase64String(configuration);
        }
    }
}