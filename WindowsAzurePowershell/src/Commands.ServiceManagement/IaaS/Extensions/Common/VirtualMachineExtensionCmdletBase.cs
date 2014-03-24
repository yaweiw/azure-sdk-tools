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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Xml;
    using System.Xml.Linq;
    using Model.PersistentVMModel;
    using Properties;

    public class VirtualMachineExtensionCmdletBase : VirtualMachineConfigurationCmdletBase
    {
        protected const string VirtualMachineExtensionNoun = "AzureVMExtension";

        protected const string ExtensionReferenceNameFormat = "{0}-{1}-{2}";
        protected const string PublicConfigurationKeyStr = "PublicConfiguration";
        protected const string PrivateConfigurationKeyStr = "PrivateConfiguration";
        protected const string PublicTypeStr = "Public";
        protected const string PrivateTypeStr = "Private";
        protected const string ReferenceDisableStr = "Disable";
        protected const string ReferenceEnableStr = "Enable";

        protected static VirtualMachineExtensionImageContext[] LegacyExtensionImages;

        protected string extensionName;
        protected string publisherName;

        public virtual string ExtensionName
        {
            get
            {
                return extensionName;
            }

            set
            {
                extensionName = value;
            }
        }

        public virtual string Publisher
        {
            get
            {
                return publisherName;
            }

            set
            {
                publisherName = value;
            }
        }

        public virtual string Version { get; set; }
        public virtual string ReferenceName { get; set; }
        public virtual string PublicConfiguration { get; set; }
        public virtual string PrivateConfiguration { get; set; }
        public virtual string PublicConfigPath { get; set; }
        public virtual string PrivateConfigPath { get; set; }
        public virtual SwitchParameter Disable { get; set; }

        static VirtualMachineExtensionCmdletBase()
        {
            LegacyExtensionImages = new VirtualMachineExtensionImageContext[2]
            {
                new VirtualMachineExtensionImageContext
                {
                    ExtensionName = "VMAccessAgent",
                    Publisher = "Microsoft.Compute",
                    Version = "0.1"
                },

                new VirtualMachineExtensionImageContext
                {
                    ExtensionName = "DiagnosticsAgent",
                    Publisher = "Microsoft.Compute",
                    Version = "0.1"
                }
            };
        }

        protected bool IsLegacyExtension()
        {
            return IsLegacyExtension(this.ExtensionName, this.Publisher, this.Version);
        }

        protected bool IsLegacyExtension(string name, string publisher, string version)
        {
            Func<string, string, bool> eq =
                (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

            return LegacyExtensionImages == null ? false
                 : LegacyExtensionImages.Any(r => eq(r.ExtensionName, name)
                                               && eq(r.Publisher, publisher)
                                               && eq(r.Version, version));
        }

        protected ResourceExtensionReferenceList ResourceExtensionReferences
        {
            get
            {
                if (VM.GetInstance().ResourceExtensionReferences == null)
                {
                    VM.GetInstance().ResourceExtensionReferences = new ResourceExtensionReferenceList();
                }

                return VM.GetInstance().ResourceExtensionReferences;
            }
        }

        protected Func<ResourceExtensionReference, bool> ExtensionPredicate
        {
            get
            {
                Func<string, string, bool> eq =
                    (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

                return string.IsNullOrEmpty(this.ExtensionName) ?
                       (Func<ResourceExtensionReference, bool>)
                       (r => eq(r.ReferenceName, this.ReferenceName))
                     : (r => eq(r.Name, this.ExtensionName)
                          && eq(r.Publisher, this.Publisher));
            }
        }

        protected List<ResourceExtensionReference> GetPredicateExtensionList()
        {
            List<ResourceExtensionReference> extensionRefs = null;
            if (!ResourceExtensionReferences.Any())
            {
                WriteWarning(Resources.ResourceExtensionReferencesIsNullOrEmpty);
                return extensionRefs;
            }

            extensionRefs = ResourceExtensionReferences.FindAll(
                r => ExtensionPredicate(r));
            if (!extensionRefs.Any())
            {
                WriteWarning(Resources.ResourceExtensionReferenceCannotBeFound);
            }

            return extensionRefs;
        }

        protected ResourceExtensionReference GetPredicateExtension()
        {
            ResourceExtensionReference extensionRef = null;
            if (!ResourceExtensionReferences.Any())
            {
                WriteWarning(Resources.ResourceExtensionReferencesIsNullOrEmpty);
            }
            else
            {
                extensionRef = ResourceExtensionReferences.FirstOrDefault(ExtensionPredicate);
            }

            return extensionRef;
        }

        protected void AddResourceExtension()
        {
            ResourceExtensionReferences.Add(NewResourceExtension());
        }

        protected void RemovePredicateExtensions()
        {
            ResourceExtensionReferences.RemoveAll(r => ExtensionPredicate(r));
        }

        protected ResourceExtensionReference NewResourceExtension()
        {
            var extensionRef = new ResourceExtensionReference();

            extensionRef.Name = this.ExtensionName;
            extensionRef.Publisher = this.Publisher;
            extensionRef.Version = this.Version;
            extensionRef.State = IsLegacyExtension() ? null :
                              this.Disable.IsPresent ? ReferenceDisableStr : ReferenceEnableStr;
            extensionRef.ResourceExtensionParameterValues = new ResourceExtensionParameterValueList();

            if (!string.IsNullOrEmpty(this.ReferenceName))
            {
                extensionRef.ReferenceName = this.ReferenceName;
            }
            else
            {
                extensionRef.ReferenceName = extensionRef.Name;
            }

            if (!string.IsNullOrEmpty(this.PublicConfigPath))
            {
                this.PublicConfiguration = File.ReadAllText(this.PublicConfigPath);
            }

            if (!string.IsNullOrEmpty(this.PublicConfiguration))
            {
                extensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key = ExtensionName + (IsLegacyExtension() ? string.Empty : PublicTypeStr) + "ConfigParameter",
                        Type = IsLegacyExtension() ? null : PublicTypeStr,
                        Value = PublicConfiguration
                    });
            }

            if (!string.IsNullOrEmpty(this.PrivateConfigPath))
            {
                this.PrivateConfiguration = File.ReadAllText(this.PrivateConfigPath);
            }

            if (!string.IsNullOrEmpty(this.PrivateConfiguration))
            {
                extensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key =  ExtensionName + (IsLegacyExtension() ? string.Empty : PrivateTypeStr) + "ConfigParameter",
                        Type = IsLegacyExtension() ? null : PrivateTypeStr,
                        Value = PrivateConfiguration
                    });
            }

            return extensionRef;
        }

        protected string GetConfiguration(
            ResourceExtensionParameterValueList paramValList,
            string typeStr)
        {
            string config = string.Empty;
            if (paramValList != null && paramValList.Any())
            {
                var paramVal = paramValList.FirstOrDefault(
                    p => string.IsNullOrEmpty(typeStr) ? true :
                         string.Equals(p.Type, typeStr, StringComparison.OrdinalIgnoreCase));
                config = paramVal == null ? string.Empty : paramVal.Value;
            }

            return config;
        }

        protected string GetConfiguration(
            ResourceExtensionReference extensionRef)
        {
            return extensionRef == null ? string.Empty : GetConfiguration(
                extensionRef.ResourceExtensionParameterValues, null);
        }

        protected string GetConfiguration(
            ResourceExtensionReference extensionRef,
            string typeStr)
        {
            return extensionRef == null ? string.Empty : GetConfiguration(
                extensionRef.ResourceExtensionParameterValues,
                typeStr);
        }

        protected virtual void ValidateParameters()
        {
            // GA must be enabled before setting extensions
            if (VM.GetInstance().ProvisionGuestAgent != null && !VM.GetInstance().ProvisionGuestAgent.Value)
            {
                throw new ArgumentException(Resources.ProvisionGuestAgentMustBeEnabledBeforeSettingIaaSVMAccessExtension);
            }
        }

        protected static string GetConfigValue(string xmlText, string element)
        {
            XDocument config = XDocument.Parse(xmlText);

            var result = from d in config.Descendants()
                         where d.Name.LocalName == element
                         select d.Descendants().Any() ? d.ToString() : d.Value;

            return result.FirstOrDefault();
        }
    }
}
