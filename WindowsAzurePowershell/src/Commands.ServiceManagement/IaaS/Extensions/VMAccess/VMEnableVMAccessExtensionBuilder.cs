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
    using System.Linq;
    using System.Xml.Linq;
    using Model.PersistentVMModel;
    using System;
    using System.Xml;

    public class VMEnableVMAccessExtensionBuilder
    {
        public const string ExtensionDefaultReferenceName = "MyPasswordResetExtension";
        public const string ExtensionDefaultPublisher = "Microsoft.Compute";
        public const string ExtensionDefaultName = "VMAccessAgent";
        public const string CurrentExtensionVersion = "0.1";
        public const string ExtensionReferenceKeyStr = "VMAccessAgentConfigParameter";

        private const string ConfigurationElem = "Configuration";
        private const string EnabledElem = "Enabled";
        private const string PublicElem = "Public";
        private const string PublicConfigElem = "PublicConfig";
        private const string AccountElem = "Account";
        private const string UserNameElem = "UserName";
        private const string PasswordElem = "Password";

        public VMEnableVMAccessExtensionBuilder()
        {
            this.Enabled = false;
        }

        public VMEnableVMAccessExtensionBuilder(string userName, string password)
        {
            this.Enabled = true;
            this.UserName = userName;
            this.Password = password;
        }
        public VMEnableVMAccessExtensionBuilder(string extensionCfg)
        {
            if (string.IsNullOrEmpty(extensionCfg))
            {
                throw new ArgumentNullException("extensionCfg");
            }

            LoadFrom(extensionCfg);
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }

        public bool Enabled
        {
            get;
            private set;
        }

        internal static string GetEnableVMAccessAgentConfig(bool enabled, string userName, string password)
        {
            XDocument publicCfg = null;
            if (enabled)
            {
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, enabled.ToString().ToLower()),
                        new XElement(PublicElem,
                            new XElement(PublicConfigElem,
                                new XElement(AccountElem,
                                    new XElement(UserNameElem, userName),
                                    new XElement(PasswordElem, password)
                                )
                            )
                        )
                    )
                );
            }
            else
            {
                publicCfg = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ConfigurationElem,
                        new XElement(EnabledElem, enabled.ToString().ToLower())
                    )
                );
            }

            return publicCfg.ToString();
        }

        public Model.PersistentVMModel.ResourceExtensionReference GetResourceReference()
        {
            return new Model.PersistentVMModel.ResourceExtensionReference
            {
                ReferenceName = ExtensionDefaultReferenceName,
                Publisher = ExtensionDefaultPublisher,
                Name = ExtensionDefaultName,
                Version = CurrentExtensionVersion,
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList(new int[1].Select(i => new Model.PersistentVMModel.ResourceExtensionParameterValue
                {
                    Key = ExtensionReferenceKeyStr,
                    Value = GetEnableVMAccessAgentConfig(
                        this.Enabled,
                        this.UserName,
                        this.Password)
                }))
            };
        }

        private static void SetConfigValue(XDocument config, string element, Object value)
        {
            if (config != null && value != null)
            {
                var ds = config.Descendants();
                foreach (var e in ds)
                {
                    if (e.Name.LocalName == element)
                    {
                        if (value.GetType().Equals(typeof(XmlDocument)))
                        {
                            e.ReplaceAll(XElement.Load(new XmlNodeReader(value as XmlDocument)));

                            var es = e.Descendants();
                            foreach (var d in es)
                            {
                                if (string.IsNullOrEmpty(d.Name.NamespaceName))
                                {
                                    d.Name = e.Name.Namespace + d.Name.LocalName;
                                }
                            };
                        }
                        else
                        {
                            e.SetValue(value.ToString());
                        }
                        break;
                    }
                };
            }
        }

        private static string GetConfigValue(string xmlText, string element)
        {
            XDocument config = XDocument.Parse(xmlText);
            var result = from d in config.Descendants()
                         where d.Name.LocalName == element
                         select d.Descendants().Any() ? d.ToString() : d.Value;
            return result.FirstOrDefault();
        }

        private void LoadFrom(string extensionCfg)
        {
            this.Enabled = bool.Parse(GetConfigValue(extensionCfg, EnabledElem).ToLower());
            this.UserName = GetConfigValue(extensionCfg, UserNameElem);
            this.Password = GetConfigValue(extensionCfg, PasswordElem);
        }
    }
}
