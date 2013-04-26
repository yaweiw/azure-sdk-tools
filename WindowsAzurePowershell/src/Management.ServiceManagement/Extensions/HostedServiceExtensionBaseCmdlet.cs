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
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    public abstract class HostedServiceExtensionBaseCmdlet : ServiceManagementBaseCmdlet
    {
        protected const int ExtensionIdLiveCycleCount = HostedServiceExtensionManager.ExtensionIdLiveCycleCount;
        protected const string PublicConfigStr = "PublicConfig";
        protected const string PrivateConfigStr = "PrivateConfig";

        protected HostedServiceExtensionManager ExtensionManager;

        protected string ExtensionNameSpace;
        protected string ExtensionType;

        protected string PublicConfigurationDescriptionTemplate;
        protected string ExtensionIdTemplate;

        protected XDocument PublicConfigurationXmlTemplate;
        protected XDocument PrivateConfigurationXmlTemplate;

        public HostedServiceExtensionBaseCmdlet()
            : base()
        {
        }

        public HostedServiceExtensionBaseCmdlet(IServiceManagement channel)
            : base()
        {
            Channel = channel;
        }

        public virtual string ServiceName
        {
            get;
            set;
        }

        protected virtual bool CheckExtensionType(HostedServiceExtensionContext extensionContext)
        {
            return extensionContext != null && extensionContext.ProviderNameSpace == ExtensionNameSpace && extensionContext.Type == ExtensionType;
        }

        protected virtual bool CheckExtensionType(HostedServiceExtension extension)
        {
            return extension != null && extension.ProviderNameSpace == ExtensionNameSpace && extension.Type == ExtensionType;
        }

        protected virtual bool CheckExtensionType(ExtensionImage extensionImage)
        {
            return extensionImage != null && extensionImage.ProviderNameSpace == ExtensionNameSpace && extensionImage.Type == ExtensionType;
        }

        protected virtual bool CheckExtensionType(Extension extension)
        {
            return extension == null ? false : CheckExtensionType(extension.Id);
        }

        protected virtual bool CheckExtensionType(string extensionId)
        {
            if (!string.IsNullOrEmpty(extensionId))
            {
                HostedServiceExtension ext = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
                return CheckExtensionType(ext);
            }
            return false;
        }

        protected virtual bool IsServiceAvailable(string serviceName)
        {
            // Check that cloud service exists
            bool found = false;
            InvokeInOperationContext(() =>
            {
                this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, serviceName).Result);
            });
            return found;
        }

        protected string GetValue(string configStr, string settingElemStr)
        {
            XDocument config = XDocument.Parse(configStr);
            var result = from configElem in config.Descendants(PublicConfigStr)
                         from settingElem in configElem.Descendants(settingElemStr)
                         where settingElem.Name == settingElemStr
                         select settingElem.Value;
            return result.FirstOrDefault();
        }
    }
}
