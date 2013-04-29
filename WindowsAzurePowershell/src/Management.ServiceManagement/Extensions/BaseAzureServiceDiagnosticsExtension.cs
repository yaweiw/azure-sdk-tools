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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System.Xml.Linq;
    using WindowsAzure.ServiceManagement;

    public abstract class BaseAzureServiceDiagnosticsExtensionCmdlet : BaseAzureServiceExtensionCmdlet
    {
        protected const string ConnectionQualifiersElemStr = "ConnectionQualifiers";
        protected const string DefaultEndpointsProtocolElemStr = "DefaultEndpointsProtocol";
        protected const string StorageAccountElemStr = "StorageAccount";
        protected const string StorageNameElemStr = "Name";
        protected const string StorageKeyElemStr = "StorageKey";
        protected const string WadCfgElemStr = "WadCfg";

        public BaseAzureServiceDiagnosticsExtensionCmdlet()
            : base()
        {
            Initialize();
        }

        public BaseAzureServiceDiagnosticsExtensionCmdlet(IServiceManagement channel)
            : base(channel)
        {
            Initialize();
        }

        protected void Initialize()
        {
            ExtensionNameSpace = "Microsoft.Windows.Azure.Extensions";
            ExtensionType = "Diagnostics";

            XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";

            PublicConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(configNameSpace + PublicConfigStr,
                    new XElement(configNameSpace + StorageAccountElemStr,
                        new XElement(configNameSpace + ConnectionQualifiersElemStr, "{0}"),
                        new XElement(configNameSpace + DefaultEndpointsProtocolElemStr, "{1}"),
                        new XElement(configNameSpace + StorageNameElemStr, "{2}")
                    ),
                    new XElement(configNameSpace + WadCfgElemStr, "{3}")
                )
            );

            PrivateConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XProcessingInstruction("xml-stylesheet", @"type=""text/xsl"" href=""style.xsl"""),
                new XElement(PrivateConfigStr,
                    new XElement(StorageKeyElemStr, "{0}")
                )
            );
        }
    }
}
