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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    using Properties;
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;

    public abstract class BaseAzureServiceDiagnosticsExtensionCmdlet : BaseAzureServiceExtensionCmdlet
    {
        protected const string StorageAccountElemStr = "StorageAccount";
        protected const string StorageNameElemStr = "Name";
        protected const string StorageNameAttrStr = "name";
        protected const string PrivConfNameAttr = "name";
        protected const string PrivConfKeyAttr = "key";
        protected const string PrivConfEndpointAttr = "endpoint";
        protected const string StorageKeyElemStr = "StorageKey";
        protected const string WadCfgElemStr = "WadCfg";
        protected const string DiagnosticsExtensionNamespace = "Microsoft.Azure.Diagnostics";
        protected const string DiagnosticsExtensionType = "PaaSDiagnostics";

        protected string StorageKey { get; set; }
        protected string ConnectionQualifiers { get; set; }
        protected string DefaultEndpointsProtocol { get; set; }
        protected string Endpoint { get; set; }

        public virtual string StorageAccountName { get; set; }
        public virtual XmlDocument DiagnosticsConfiguration { get; set; }


        public BaseAzureServiceDiagnosticsExtensionCmdlet()
            : base()
        {
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();

            XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
            ProviderNamespace = DiagnosticsExtensionNamespace;
            ExtensionName = DiagnosticsExtensionType;
            PublicConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(configNameSpace + PublicConfigStr,
                    new XElement(configNameSpace + WadCfgElemStr, string.Empty),
                    new XElement(configNameSpace + StorageAccountElemStr, string.Empty)
                )
            );

            PrivateConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(configNameSpace + PrivateConfigStr,
                    new XElement(configNameSpace + StorageAccountElemStr,
                    new XAttribute(PrivConfNameAttr, string.Empty),
                    new XAttribute(PrivConfKeyAttr, string.Empty),
                    new XAttribute(PrivConfEndpointAttr, string.Empty)
                ))
            );
        }

        protected void ValidateStorageAccount()
        {
            var storageService = this.StorageClient.StorageAccounts.Get(StorageAccountName);
            if (storageService == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountName, StorageAccountName));
            }

            var storageKeys = this.StorageClient.StorageAccounts.GetKeys(storageService.StorageAccount.Name);
            if (storageKeys == null || storageKeys.PrimaryKey == null || storageKeys.SecondaryKey == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountKey, StorageAccountName));
            }
            StorageKey = storageKeys.PrimaryKey != null ? storageKeys.PrimaryKey : storageKeys.SecondaryKey;

            CloudStorageAccount srcStorageAccount = new CloudStorageAccount(new StorageCredentials(StorageAccountName, StorageKey), true);
            Endpoint = srcStorageAccount.TableStorageUri.PrimaryUri.ToString();


            // the endpoint format for table is:  http://aaaaaa.table.xxxxxx.yyy where aaaaaa is the StorageAccountName.
            // we really want to get rid of aaaaaa.table and keep what is left: http://xxxxxx.yyy

            int tableIndex = Endpoint.IndexOf(StorageAccountName);
            if (tableIndex < 0)
            {
                throw new Exception(string.Format("Cannot find the storage account name \"{0}\" in the endpoint \"{1)\"", StorageAccountName, Endpoint));
            }

            tableIndex += StorageAccountName.Length + 1; // +1 for the dot after the storage account name

            int slashIndex = Endpoint.IndexOf("//");
            if (slashIndex < 0)
            {
                throw new Exception(string.Format("Cannot find the \"\\\" in the endpoint \"{0)\"", Endpoint));
            }

            Endpoint = Endpoint.Substring(0, slashIndex + 2) + Endpoint.Substring(tableIndex + "table.".Length);
        }

        protected override void ValidateConfiguration()
        {
            PublicConfigurationXml = new XDocument(PublicConfigurationXmlTemplate);
            SetPublicConfigValue(WadCfgElemStr, DiagnosticsConfiguration);
            SetPublicConfigValue(StorageAccountElemStr, StorageAccountName);
            PublicConfiguration = PublicConfigurationXml.ToString();

            PrivateConfigurationXml = new XDocument(PrivateConfigurationXmlTemplate);
            SetPrivateConfigAttribute(StorageAccountElemStr, PrivConfNameAttr, StorageAccountName);
            SetPrivateConfigAttribute(StorageAccountElemStr, PrivConfKeyAttr, StorageKey);
            SetPrivateConfigAttribute(StorageAccountElemStr, PrivConfEndpointAttr, Endpoint);
            PrivateConfiguration = PrivateConfigurationXml.ToString();

        }
    }
}