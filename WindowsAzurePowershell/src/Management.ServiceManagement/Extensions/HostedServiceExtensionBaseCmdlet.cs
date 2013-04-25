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
    using System.Xml;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    public abstract class HostedServiceExtensionBaseCmdlet : ServiceManagementBaseCmdlet
    {
        protected const int ExtensionIdLiveCycleCount = 2;

        protected HostedServiceExtensionManager ExtensionManager;

        protected string LegacySettingStr;
        protected string ExtensionNameSpace;
        protected string ExtensionType;
        protected string PublicConfigurationTemplate;
        protected string PrivateConfigurationTemplate;
        protected string PublicConfigurationDescriptionTemplate;
        protected string ExtensionIdTemplate;

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

        protected virtual bool IsLegacySettingEnabled(Deployment deployment)
        {
            bool enabled = false;
            if (deployment != null && deployment.Configuration != null)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(deployment.Configuration);
                    XmlNodeList xmlNodeList = xmlDoc.GetElementsByTagName("ConfigurationSettings");
                    if (xmlNodeList.Count > 0)
                    {
                        XmlNode parentNode = xmlNodeList.Item(0);
                        foreach (XmlNode childNode in parentNode)
                        {
                            if (childNode.Attributes["name"] != null && childNode.Attributes["value"] != null)
                            {
                                string nameStr = childNode.Attributes["name"].Value;
                                string valueStr = childNode.Attributes["value"].Value;
                                if (nameStr.Equals(LegacySettingStr))
                                {
                                    enabled = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot determine deployment legacy setting - configuration parsing error: " + ex.Message);
                }
            }
            return enabled;
        }
    }
}
