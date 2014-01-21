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
namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Model.PersistentVMModel;
    using Utilities.Common;

    public class VirtualMachineDiagnosticsExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
        public const string EnableExtensionUsingXmlDocumentParameterSet = "EnableExtensionUsingXmlDocument";
        public const string EnableExtensionUsingXmlFilePathParameterSet = "EnableExtensionUsingXmlFilePath";
        public const string EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext = "EnableExtensionUsingXmlDocumentUsingStorageContext";
        public const string EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext = "EnableExtensionUsingXmlFilePathUsingStorageContext";
        public const string DisableExtensionParameterSet = "DisableExtension";

        protected const string VirtualMachineDiagnosticsExtensionNoun = "AzureVMDiagnosticsExtension";

        protected const string ExtensionDefaultPublisher = "Microsoft.Compute";
        protected const string ExtensionDefaultName = "DiagnosticsAgent";
        protected const string CurrentExtensionVersion = "0.1";

        protected const string ExtensionDefaultReferenceName = "MyDiagnosticsAgent";
        protected const string ExtensionReferenceKeyStr = "DiagnosticsAgentConfigParameter";

        private const string ConfigurationElem = "Configuration";
        private const string EnabledElem = "Enabled";
        private const string PublicElem = "Public";
        private const string PublicConfigElem = "PublicConfig";
        private const string WadCfgElem = "WadCfg";
        private const string DiagnosticMonitorConfigurationElem = "DiagnosticMonitorConfiguration";
        private const string StorageAccountConnectionStringElem = "StorageAccountConnectionString";

        private const string DefaultEndpointsProtocol = "https";
        private const string StorageConnectionStringFormat = "DefaultEndpointsProtocol={0};AccountName={1};AccountKey={2};{3}";

        public virtual string StorageAccountName { get; set; }

        public virtual string StorageAccountKey { get; set; }

        public virtual Uri[] Endpoints { get; set; }

        public virtual bool Enabled { get; set; }

        public virtual XmlDocument DiagnosticsConfiguration { get; set; }

        public virtual string DiagnosticsConfigurationFile { get; set; }

        public virtual string EndpointSuffix { get; set; }

        public virtual StorageServicePropertiesOperationContext StorageAccountContext { get; set; }

        public VirtualMachineDiagnosticsExtensionCmdletBase()
        {
            Publisher = ExtensionDefaultPublisher;
            ExtensionName = ExtensionDefaultName;
        }

        protected string GetDiagnosticsAgentConfig()
        {
            XDocument publicCfg = null;
            if (Enabled)
            {
                XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, Enabled.ToString().ToLower()),
                        new XElement(PublicElem,
                            new XElement(configNameSpace + PublicConfigElem,
                                new XElement(configNameSpace + WadCfgElem, string.Empty),
                                new XElement(configNameSpace + StorageAccountConnectionStringElem, string.Empty)
                            )
                        )
                    )
                );

                var cloudStorageCredential = new StorageCredentials(StorageAccountName, StorageAccountKey);
                var cloudStorageAccount = Endpoints == null ? new CloudStorageAccount(cloudStorageCredential, true)
                                                            : new CloudStorageAccount(cloudStorageCredential, Endpoints[0], Endpoints[1], Endpoints[2]); // {blob, queue, table}
                var storageConnectionStr = cloudStorageAccount.ToString(true);

                SetConfigValue(publicCfg, WadCfgElem, DiagnosticsConfiguration);
                SetConfigValue(publicCfg, StorageAccountConnectionStringElem, storageConnectionStr);
            }
            else
            {
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, Enabled.ToString().ToLower())
                    )
                );
            }

            return publicCfg.ToString();
        }

        protected void GetDiagnosticsAgentValues(ResourceExtensionParameterValueList paramVals)
        {
            if (paramVals != null && paramVals.Any())
            {
                GetDiagnosticsAgentValues(paramVals.FirstOrDefault(r => !string.IsNullOrEmpty(r.Value)));
            }
        }

        protected void GetDiagnosticsAgentValues(ResourceExtensionParameterValue paramVal)
        {
            if (paramVal != null && !string.IsNullOrEmpty(paramVal.Value))
            {
                GetDiagnosticsAgentValues(paramVal.Value);
            }
        }

        protected void GetDiagnosticsAgentValues(string extensionCfg)
        {
            this.Enabled = bool.Parse(GetConfigValue(extensionCfg, EnabledElem).ToLower());

            var storageConnectionString = GetConfigValue(extensionCfg, StorageAccountConnectionStringElem);
            var cloudStorageAccount = string.IsNullOrEmpty(storageConnectionString) ? null : CloudStorageAccount.Parse(storageConnectionString);
            if (cloudStorageAccount != null && cloudStorageAccount.Credentials != null)
            {
                this.StorageAccountName = cloudStorageAccount.Credentials.AccountName;
                var exportedKey = cloudStorageAccount.Credentials.ExportKey();
                this.StorageAccountKey = exportedKey == null ? null : Convert.ToBase64String(exportedKey);
                this.Endpoints = new Uri[3] { cloudStorageAccount.BlobEndpoint, cloudStorageAccount.QueueEndpoint, cloudStorageAccount.TableEndpoint };
            }
            else
            {
                this.StorageAccountName = this.StorageAccountKey = null;
                this.Endpoints = null;
            }

            var daCfgContent = GetConfigValue(extensionCfg, DiagnosticMonitorConfigurationElem);
            if (!string.IsNullOrEmpty(daCfgContent))
            {
                XDocument daCfgDoc = XDocument.Parse(daCfgContent);
                daCfgDoc.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();
                daCfgDoc.Descendants().ForEach(e => e.Name = e.Name.LocalName);
                this.DiagnosticsConfiguration = new XmlDocument();
                this.DiagnosticsConfiguration.LoadXml(daCfgDoc.ToString());
            }
            else
            {
                this.DiagnosticsConfiguration = null;
            }
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();

            // Validate DA Extension related parameters.
            if (ParameterSetName != DisableExtensionParameterSet)
            {
                if (!string.IsNullOrEmpty(DiagnosticsConfigurationFile))
                {
                    DiagnosticsConfiguration = new XmlDocument();
                    DiagnosticsConfiguration.LoadXml(General.GetConfiguration(DiagnosticsConfigurationFile));
                }
            }

            if (ParameterSetName == EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext ||
                ParameterSetName == EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext)
            {
                this.StorageAccountName = this.StorageAccountContext.StorageAccountName;
                // must be in this order: blob, queue, table endpoints
                this.Endpoints = this.StorageAccountContext.Endpoints == null || this.StorageAccountContext.Endpoints.Count() == 0 ? null :
                                 this.StorageAccountContext.Endpoints.Select(e => new Uri(e)).ToArray();
            }
            else if (ParameterSetName == EnableExtensionUsingXmlDocumentParameterSet ||
                     ParameterSetName == EnableExtensionUsingXmlFilePathParameterSet)
            {
                if (!string.IsNullOrEmpty(this.EndpointSuffix))
                {
                    string endpointFormat = "https://{0}.{1}.{2}";
                    this.Endpoints = new string[3] { "blob", "queue", "table" }.Select(
                        s => new Uri(string.Format(endpointFormat, this.StorageAccountName, s, this.EndpointSuffix))).ToArray();
                }
            }
        }
    }
}
