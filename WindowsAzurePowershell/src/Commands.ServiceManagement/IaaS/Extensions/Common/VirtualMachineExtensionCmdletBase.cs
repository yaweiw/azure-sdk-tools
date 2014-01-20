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
    using System.Management.Automation;
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

        public virtual string ExtensionName { get; set; }
        public virtual string Publisher { get; set; }
        public virtual string Version { get; set; }
        public virtual string ReferenceName { get; set; }
        public virtual string PublicConfiguration { get; set; }
        public virtual string PrivateConfiguration { get; set; }
        public virtual SwitchParameter Disable { get; set; }

        protected static VirtualMachineExtensionImageContext[] LegacyExtensionImages;

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
            Func<string, string, bool> eq =
                (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

            return LegacyExtensionImages == null ? false
                 : LegacyExtensionImages.Any(r => eq(r.ExtensionName, ExtensionName)
                                               && eq(r.Publisher, this.Publisher)
                                               && eq(r.Version, this.Version));
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
                      : r => eq(r.Name, this.ExtensionName)
                          && eq(r.Publisher, this.Publisher)
                          && eq(r.Version, this.Version);
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
                return extensionRef;
            }

            extensionRef =
                ResourceExtensionReferences.FirstOrDefault(ExtensionPredicate);
            if (extensionRef == null)
            {
                throw new ArgumentNullException(
                    "VM.ResourceExtensionReferences",
                    Resources.ResourceExtensionReferenceCannotBeFound);
            }

            return extensionRef;
        }

        protected ResourceExtensionReference NewResourceExtension()
        {
            return SetResourceExtension(new ResourceExtensionReference());
        }

        protected ResourceExtensionReference SetResourceExtension(ResourceExtensionReference extensionRef)
        {
            if (extensionRef == null)
            {
                throw new ArgumentNullException("extensionRef");
            }

            extensionRef.Name = this.ExtensionName;
            extensionRef.Publisher = this.Publisher;
            extensionRef.Version = this.Version;
            extensionRef.State = IsLegacyExtension() ? null : this.Disable.IsPresent ? ReferenceDisableStr : ReferenceEnableStr;
            extensionRef.ResourceExtensionParameterValues = new ResourceExtensionParameterValueList();

            if (!string.IsNullOrEmpty(this.ReferenceName))
            {
                extensionRef.ReferenceName = this.ReferenceName;
            }
            else
            {
                extensionRef.ReferenceName = string.Format(
                    ExtensionReferenceNameFormat,
                    extensionRef.Publisher,
                    extensionRef.Name,
                    extensionRef.Version);
            }

            if (!string.IsNullOrEmpty(this.PublicConfiguration))
            {
                extensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key = ExtensionName + "ConfigParameter",
                        Type = IsLegacyExtension() ? null : PublicTypeStr,
                        Value = PublicConfiguration
                    });
            }

            if (!string.IsNullOrEmpty(this.PrivateConfiguration))
            {
                extensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key = ExtensionName + "ConfigParameter",
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
                    p => string.Equals(p.Type, typeStr, StringComparison.OrdinalIgnoreCase));
                config = paramVal == null ? string.Empty : paramVal.Value;
            }

            return config;
        }

        protected string GetConfiguration(
            ResourceExtensionReference extensionRef,
            string typeStr)
        {
            return extensionRef == null ? string.Empty : GetConfiguration(
                extensionRef.ResourceExtensionParameterValues,
                typeStr);
        }

        protected void ValidateParameters()
        {
            // GA must be enabled before setting WAD
            if (VM.GetInstance().ProvisionGuestAgent != null && !VM.GetInstance().ProvisionGuestAgent.Value)
            {
                throw new ArgumentException(Resources.ProvisionGuestAgentMustBeEnabledBeforeSettingIaaSVMAccessExtension);
            }
        }
    }
}
