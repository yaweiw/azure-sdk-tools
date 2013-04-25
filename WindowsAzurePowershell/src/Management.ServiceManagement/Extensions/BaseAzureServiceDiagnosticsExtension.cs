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
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    public abstract class BaseAzureServiceDiagnosticsExtensionCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string LegacySettingStr = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        protected const string ExtensionNameSpace = "Microsoft.Windows.Azure.Extensions";
        protected const string ExtensionType = "Diagnostics";

        protected const string PublicConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                             "<PublicConfig xmlns=\"http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration\">" +
                                                             "<StorageAccount>" +
                                                             "<ConnectionQualifiers>" + "{0}" + "</ConnectionQualifiers>" +
                                                             "<DefaultEndpointsProtocol>" + "{1}" + "</DefaultEndpointsProtocol>" +
                                                             "<Name>" + "{2}" + "</Name>" +
                                                             "</StorageAccount>" +
                                                             "<WadCfg />" +
                                                             "</PublicConfig>";

        protected const string PrivateConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                              "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                                              "<PrivateConfig>" +
                                                              "<StorageKey>" + "{0}" + "</StorageKey>" +
                                                              "</PrivateConfig>";

        protected const string PublicConfigurationDescriptionTemplate = "Diagnostics Enabled ConnectionQualifiers: {0}, DefaultEndpointsProtocol: {1}, Name: {2}";

        protected const string ExtensionIdTemplate = "{0}-Diagnostics-Ext-{1}";

        protected const int ExtensionIdLiveCycleCount = 2;

        protected bool CheckExtensionType(HostedServiceExtensionContext extensionContext)
        {
            return extensionContext != null && extensionContext.ProviderNameSpace == ExtensionNameSpace && extensionContext.Type == ExtensionType;
        }

        protected bool CheckExtensionType(HostedServiceExtension extension)
        {
            return extension != null && extension.ProviderNameSpace == ExtensionNameSpace && extension.Type == ExtensionType;
        }

        protected bool CheckExtensionType(ExtensionImage extensionImage)
        {
            return extensionImage != null && extensionImage.ProviderNameSpace == ExtensionNameSpace && extensionImage.Type == ExtensionType;
        }

        protected bool CheckExtensionType(Extension extension)
        {
            return extension == null ? false : CheckExtensionType(extension.Id);
        }

        protected abstract bool CheckExtensionType(string extensionId);

        protected bool IsServiceAvailable(string serviceName)
        {
            // Check that cloud service exists
            bool found = false;
            InvokeInOperationContext(() =>
            {
                this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, serviceName).Result);
            });
            return found;
        }
    }
}
