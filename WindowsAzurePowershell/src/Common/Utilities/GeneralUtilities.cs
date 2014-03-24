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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using CloudService;
    using Commands.Common.Properties;
    using XmlSchema.ServiceConfigurationSchema;

    public static class GeneralUtilities
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

        public static string GetNodeModulesPath()
        {
            return Path.Combine(FileUtilities.GetAssemblyDirectory(), Resources.NodeModulesPath);
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
            if (certificate != null)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(certificate);
                store.Close();
            }
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

        public static string ReadMessageBody(ref Message originalMessage)
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

            return XmlUtilities.Beautify(strBuilder.ToString());
        }

        public static string GetConfiguration(string configurationPath)
        {
            var configuration = string.Join(string.Empty, File.ReadAllLines(configurationPath));
            return configuration;
        }

        /// <summary>
        /// Get the value for a given key in a dictionary or return a default
        /// value if the key isn't present in the dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the key.</typeparam>
        /// <typeparam name="V">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">A default value</param>
        /// <returns>The corresponding value or default value.</returns>
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue)
        {
            Debug.Assert(dictionary != null, "dictionary cannot be null!");

            V value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Returns a non-null sequence by either passing back the original
        /// sequence or creating a new empty sequence if the original was null.
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <returns>A non-null sequence.</returns>
        public static IEnumerable<T> NonNull<T>(this IEnumerable<T> sequence)
        {
            return (sequence != null) ?
                sequence :
                Enumerable.Empty<T>();
        }

        /// <summary>
        /// Perform an action on each element of a sequence.
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="action">The action to perform.</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            Debug.Assert(sequence != null, "sequence cannot be null!");
            Debug.Assert(action != null, "action cannot be null!");

            foreach (T element in sequence)
            {
                action(element);
            }
        }

        /// <summary>
        /// Append an element to the end of an array.
        /// </summary>
        /// <typeparam name="T">Type of the arrays.</typeparam>
        /// <param name="left">The left array.</param>
        /// <param name="right">The right array.</param>
        /// <returns>The concatenated arrays.</returns>
        public static T[] Append<T>(T[] left, T right)
        {
            if (left == null)
            {
                return right != null ?
                    new T[] { right } :
                    new T[] { };
            }
            else if (right == null)
            {
                return left;
            }
            else
            {
                return Enumerable.Concat(left, new T[] { right }).ToArray();
            }
        }

        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> sequence, Func<T, TResult> selector, TResult defaultValue)
        {
            return (sequence != null) ? sequence.Max(selector) : defaultValue;
        }

        /// <summary>
        /// Extends the array with one element.
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="collection">The array holding elements</param>
        /// <param name="item">The item to add</param>
        /// <returns>New array with added item</returns>
        public static T[] ExtendArray<T>(IEnumerable<T> collection, T item)
        {
            if (collection == null)
            {
                collection = new T[0];
            }

            List<T> list = new List<T>(collection);
            list.Add(item);
            return list.ToArray<T>();
        }

        /// <summary>
        /// Extends the array with another array
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="collection">The array holding elements</param>
        /// <param name="items">The items to add</param>
        /// <returns>New array with added items</returns>
        public static T[] ExtendArray<T>(IEnumerable<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
            {
                collection = new T[0];
            }

            if (items == null)
            {
                items = new T[0];
            }

            return collection.Concat<T>(items).ToArray<T>();
        }

        /// <summary>
        /// Initializes given object if its set to null.
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="obj">The object to initialize</param>
        /// <returns>Initialized object</returns>
        public static T InitializeIfNull<T>(T obj)
            where T : new()
        {
            if (obj == null)
            {
                return new T();
            }

            return obj;
        }

        public static ServiceSettings GetDefaultSettings(
            string rootPath,
            string inServiceName,
            string slot,
            string location,
            string affinityGroup,
            string storageName,
            string subscription,
            out string serviceName)
        {
            ServiceSettings serviceSettings;

            if (string.IsNullOrEmpty(rootPath))
            {
                serviceSettings = ServiceSettings.LoadDefault(null, slot, location, affinityGroup, subscription, storageName, inServiceName, null, out serviceName);
            }
            else
            {
                serviceSettings = ServiceSettings.LoadDefault(
                    new CloudServiceProject(rootPath, null).Paths.Settings,
                    slot,
                    location,
                    affinityGroup,
                    subscription,
                    storageName,
                    inServiceName,
                    new CloudServiceProject(rootPath, null).ServiceName,
                    out serviceName);
            }

            return serviceSettings;
        }

        /// <summary>
        /// Gets role name for the current pathif exists.
        /// </summary>
        /// <returns>The role name</returns>
        public static string GetRoleName(string rootPath, string currentPath)
        {
            bool found = false;
            string roleName = null;

            if (!(rootPath.Length >= currentPath.Length))
            {
                string difference = currentPath.Replace(rootPath, string.Empty);
                roleName = difference.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).GetValue(0).ToString();
                CloudServiceProject service = new CloudServiceProject(rootPath, null);
                found = service.Components.RoleExists(roleName);
            }

            if (!found)
            {
                throw new ArgumentException(string.Format(Resources.CannotFindRole, currentPath));
            }

            return roleName;
        }

        /// <summary>
        /// Tries to get service path, if not return null.
        /// </summary>
        /// <param name="currentPath">The current path</param>
        /// <returns>The service root path</returns>
        public static string TryGetServiceRootPath(string currentPath)
        {
            try { return GetServiceRootPath(currentPath); }
            catch (Exception) { return null; }
        }

        public static string GetServiceRootPath(string currentPath)
        {
            // Get the service path
            string servicePath = FindServiceRootDirectory(currentPath);

            // Was the service path found?
            if (servicePath == null)
            {
                throw new InvalidOperationException(Resources.CannotFindServiceRoot);
            }

            CloudServiceProject service = new CloudServiceProject(servicePath, null);
            if (service.Components.CloudConfig.Role != null)
            {
                foreach (RoleSettings role in service.Components.CloudConfig.Role)
                {
                    string roleDirectory = Path.Combine(service.Paths.RolesPath, role.name);

                    if (!Directory.Exists(roleDirectory))
                    {
                        throw new InvalidOperationException(Resources.CannotFindServiceRoot);
                    }
                }
            }

            return servicePath;
        }

        private static string FindServiceRootDirectory(string path)
        {
            if (Directory.GetFiles(path, Resources.ServiceDefinitionFileName).Length == 1)
            {
                return path;
            }
            else if (Directory.GetFiles(path, "*.sln").Length == 1)
            {
                foreach (string dirName in Directory.GetDirectories(path))
                {
                    if (Directory.GetFiles(dirName, Resources.ServiceDefinitionFileName).Length == 1)
                    {
                        return dirName;
                    }
                }
            }

            // Find the last slash
            int slash = path.LastIndexOf('\\');
            if (slash > 0)
            {
                // Slash found trim off the last path
                path = path.Substring(0, slash);

                // Recurse
                return FindServiceRootDirectory(path);
            }

            // Couldn't locate the service root, exit
            return null;
        }

        public static string EnsureTrailingSlash(string url)
        {
            UriBuilder address = new UriBuilder(url);
            if (!address.Path.EndsWith("/", StringComparison.Ordinal))
            {
                address.Path += "/";
            }
            return address.Uri.AbsoluteUri;
        }

        public static string GetHttpResponseLog(string statusCode, WebHeaderCollection headers, string body)
        {
            StringBuilder httpResponseLog = new StringBuilder();
            httpResponseLog.AppendLine(string.Format("============================ HTTP RESPONSE ============================{0}", Environment.NewLine));
            httpResponseLog.AppendLine(string.Format("Status Code:{0}{1}{0}", Environment.NewLine, statusCode));
            httpResponseLog.AppendLine(string.Format("Headers:{0}{1}", Environment.NewLine, MessageHeadersToString(headers)));
            httpResponseLog.AppendLine(string.Format("Body:{0}{1}{0}", Environment.NewLine, body));

            return httpResponseLog.ToString();
        }

        public static string GetHttpResponseLog(string statusCode, HttpHeaders headers, string body)
        {
            return GetHttpResponseLog(statusCode, ConvertHttpHeadersToWebHeaderCollection(headers), body);
        }

        public static string GetHttpRequestLog(
            string method,
            string requestUri,
            WebHeaderCollection headers,
            string body)
        {
            StringBuilder httpRequestLog = new StringBuilder();
            httpRequestLog.AppendLine(string.Format("============================ HTTP REQUEST ============================{0}", Environment.NewLine));
            httpRequestLog.AppendLine(string.Format("HTTP Method:{0}{1}{0}", Environment.NewLine, method));
            httpRequestLog.AppendLine(string.Format("Absolute Uri:{0}{1}{0}", Environment.NewLine, requestUri));
            httpRequestLog.AppendLine(string.Format("Headers:{0}{1}", Environment.NewLine, MessageHeadersToString(headers)));
            httpRequestLog.AppendLine(string.Format("Body:{0}{1}{0}", Environment.NewLine, body));

            return httpRequestLog.ToString();
        }

        public static string GetHttpRequestLog(string method, string requestUri, HttpHeaders headers, string body)
        {
            return GetHttpRequestLog(method, requestUri, ConvertHttpHeadersToWebHeaderCollection(headers), body);
        }

        public static string GetLog(HttpResponseMessage response)
        {
            string body = response.Content == null ? string.Empty : FormatString(response.Content.ReadAsStringAsync().Result);

            return GetHttpResponseLog(
                response.StatusCode.ToString(),
                response.Headers,
                body);
        }

        public static string GetLog(HttpRequestMessage request)
        {
            string body = request.Content == null ? string.Empty : FormatString(request.Content.ReadAsStringAsync().Result);

            return GetHttpRequestLog(
                request.Method.ToString(),
                request.RequestUri.ToString(),
                (HttpHeaders)request.Headers,
                body);
        }

        public static string FormatString(string content)
        {
            if (XmlUtilities.IsXml(content))
            {
                return XmlUtilities.TryFormatXml(content);
            }
            else if (JsonUtilities.IsJson(content))
            {
                return JsonUtilities.TryFormatJson(content);
            }
            else
            {
                return content;
            }
        }

        private static WebHeaderCollection ConvertHttpHeadersToWebHeaderCollection(HttpHeaders headers)
        {
            WebHeaderCollection webHeaders = new WebHeaderCollection();
            foreach (KeyValuePair<string, IEnumerable<string>> pair in headers)
            {
                pair.Value.ForEach<string>(v => webHeaders.Add(pair.Key, v));
            }

            return webHeaders;
        }

        private static string MessageHeadersToString(WebHeaderCollection headers)
        {
            string[] keys = headers.AllKeys;
            StringBuilder result = new StringBuilder();

            foreach (string key in keys)
            {
                result.AppendLine(string.Format(
                    "{0,-30}: {1}",
                    key,
                    ConversionUtilities.ArrayToString(headers.GetValues(key), ",")));
            }

            return result.ToString();
        }

        /// <summary>
        /// Creates https endpoint from the given endpoint.
        /// </summary>
        /// <param name="endpointUri">The endpoint uri.</param>
        /// <returns>The https endpoint uri.</returns>
        public static Uri CreateHttpsEndpoint(string endpointUri)
        {
            UriBuilder builder = new UriBuilder(endpointUri) { Scheme = "https" };
            string endpoint = builder.Uri.GetComponents(
                UriComponents.AbsoluteUri & ~UriComponents.Port,
                UriFormat.UriEscaped);

            return new Uri(endpoint);
        }

        public static string GetNonEmptyValue(string oldValue, string newValue)
        {
            return string.IsNullOrEmpty(newValue) ? oldValue : newValue;
        }

        public static string DownloadFile(string uri)
        {
            string contents = null;

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    contents = webClient.DownloadString(new Uri(uri));
                }
                catch
                {
                    // Ignore the exception and return empty contents
                }
            }

            return contents;
        }

        public static string ToUpperFirstLetter(string word)
        {
            return string.IsNullOrEmpty(word) ? word : word.Substring(0, 1).ToUpper() + word.Substring(1);
        }

        public static object GetValue(dynamic variable, string property)
        {
            object value = null;
            IDictionary<String, object> expanded = (IDictionary<String, object>)variable;

            if (expanded.ContainsKey(property))
            {
                value = expanded[property];
            }

            return value;
        }
    }
}