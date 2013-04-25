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
    using WindowsAzure.ServiceManagement;

    public abstract class BaseAzureServiceRemoteDesktopExtensionCmdlet : HostedServiceExtensionBaseCmdlet
    {
        public BaseAzureServiceRemoteDesktopExtensionCmdlet()
            : base()
        {
            Initialize();
        }

        public BaseAzureServiceRemoteDesktopExtensionCmdlet(IServiceManagement channel)
            : base(channel)
        {
            Initialize();
        }

        protected void Initialize()
        {
            LegacySettingStr = "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled";
            ExtensionNameSpace = "Microsoft.Windows.Azure.Extensions";
            ExtensionType = "RDP";
            PublicConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                            "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                            "<PublicConfig>" +
                                            "<UserName>{0}</UserName>" +
                                            "<Expiration>{1}</Expiration>" +
                                            "</PublicConfig>";
            PrivateConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                            "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                            "<PrivateConfig>" +
                                            "<Password>{0}</Password>" +
                                            "</PrivateConfig>";
            PublicConfigurationDescriptionTemplate = "RDP Enabled User: {0}, Expires: {1}";
            ExtensionIdTemplate = "{0}-RDP-Ext-{1}";
        }
    }
}
