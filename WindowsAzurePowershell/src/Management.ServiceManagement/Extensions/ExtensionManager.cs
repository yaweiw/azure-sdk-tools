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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Text;
    using Properties;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    public class ExtensionManager
    {
        public const int ExtensionIdLiveCycleCount = 2;
        private const string ExtensionIdTemplate = "{0}-{1}-{2}-Ext-{3}";
        private const string DefaultAllRolesNameStr = "Default";
        private const string ExtensionCertificateSubject = "DC=Windows Azure Service Management for Extensions";
        private const string ThumbprintAlgorithmStr = "sha1";

        protected ServiceManagementBaseCmdlet Cmdlet { get; private set; }
        protected IServiceManagement Channel { get; private set; }
        protected string SubscriptionId { get; private set; }
        protected string ServiceName { get; private set; }
        protected HostedServiceExtensionList ExtendedExtensionList { get; private set; }

        public ExtensionManager(ServiceManagementBaseCmdlet cmdlet, string serviceName)
        {
            if (cmdlet == null || cmdlet.Channel == null || cmdlet.CurrentSubscription == null)
            {
                throw new ArgumentNullException("cmdlet");
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }

            Cmdlet = cmdlet;
            Channel = cmdlet.Channel;
            SubscriptionId = cmdlet.CurrentSubscription.SubscriptionId;
            ServiceName = serviceName;
        }

        public HostedServiceExtension GetExtension(string extensionId)
        {
            if (ExtendedExtensionList == null)
            {
                ExtendedExtensionList = Channel.ListHostedServiceExtensions(SubscriptionId, ServiceName);
            }
            return ExtendedExtensionList == null ? null : ExtendedExtensionList.Find(e => e.Id == extensionId);
        }

        public void DeleteExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(SubscriptionId, ServiceName, extensionId);
        }

        public void AddExtension(HostedServiceExtensionInput extension)
        {
            Channel.AddHostedServiceExtension(SubscriptionId, ServiceName, extension);
        }

        public bool CheckNameSpaceType(HostedServiceExtension extension, string nameSpace, string type)
        {
            return extension != null && extension.ProviderNameSpace == nameSpace && extension.Type == type;
        }

        public bool CheckNameSpaceType(string extensionId, string nameSpace, string type)
        {
            return !string.IsNullOrEmpty(extensionId) && CheckNameSpaceType(GetExtension(extensionId), nameSpace, type);
        }

        public ExtensionConfigurationBuilder GetBuilder()
        {
            return new ExtensionConfigurationBuilder(this);
        }

        public ExtensionConfigurationBuilder GetBuilder(ExtensionConfiguration config)
        {
            return new ExtensionConfigurationBuilder(this, config);
        }

        public string GetExtensionId(string roleName, string type, string slot, int index)
        {
            return string.Format(ExtensionIdTemplate, roleName, type, slot, index);
        }

        private void GetThumbprintAndAlgorithm(List<HostedServiceExtension> extensionList, string extensionId, ref string thumbprint, ref string thumbprintAlgorithm)
        {
            var existingExtension = extensionList.Find(e => e.Id == extensionId);
            if (existingExtension != null)
            {
                thumbprint = existingExtension.Thumbprint;
                thumbprintAlgorithm = existingExtension.ThumbprintAlgorithm;
            }
            else if (extensionList.Any())
            {
                thumbprint = extensionList.First().Thumbprint;
                thumbprintAlgorithm = extensionList.First().ThumbprintAlgorithm;
            }
            else if (ExtendedExtensionList != null && ExtendedExtensionList.Any())
            {
                thumbprint = ExtendedExtensionList.First().Thumbprint;
                thumbprintAlgorithm = ExtendedExtensionList.First().ThumbprintAlgorithm;
            }

            var certList = Channel.ListCertificates(SubscriptionId, ServiceName);
            string extThumbprint = thumbprint;
            string extThumbprintAlgorithm = thumbprintAlgorithm;
            var cert = certList.Find(c => c.Thumbprint == extThumbprint && c.ThumbprintAlgorithm == extThumbprintAlgorithm);
            cert = cert != null ? cert : certList.Find(c =>
            {
                byte[] bytes = Encoding.ASCII.GetBytes(c.Data);
                X509Certificate2 x509cert = null;
                try
                {
                    x509cert = new X509Certificate2(bytes);
                }
                catch (CryptographicException)
                {
                    // Do nothing
                }
                return x509cert != null && ExtensionCertificateSubject.Equals(x509cert.Subject);
            });

            if (cert != null)
            {
                thumbprint = cert.Thumbprint;
                thumbprintAlgorithm = cert.ThumbprintAlgorithm;
            }
            else
            {
                thumbprint = string.Empty;
                thumbprintAlgorithm = string.Empty;
            }
        }

        public ExtensionConfiguration InstallExtension(ExtensionConfigurationInput context, string slot, ExtensionConfiguration extConfig)
        {
            ExtensionConfigurationBuilder builder = GetBuilder(extConfig);
            foreach (ExtensionRole r in context.Roles)
            {
                var extensionIds = (from index in Enumerable.Range(0, ExtensionIdLiveCycleCount)
                                    select GetExtensionId(r.PrefixName, context.Type, slot, index)).ToList();

                string availableId = (from extensionId in extensionIds
                                      where !builder.ExistAny(extensionId)
                                      select extensionId).FirstOrDefault();

                var extensionList = (from id in extensionIds
                                     let e = GetExtension(id)
                                     where e != null
                                     select e).ToList();

                string thumbprint = context.CertificateThumbprint;
                string thumbprintAlgorithm = context.ThumbprintAlgorithm;

                if (context.X509Certificate != null)
                {
                    thumbprint = context.X509Certificate.Thumbprint;
                }
                else
                {
                    GetThumbprintAndAlgorithm(extensionList, availableId, ref thumbprint, ref thumbprintAlgorithm);
                }

                context.CertificateThumbprint = string.IsNullOrWhiteSpace(context.CertificateThumbprint) ? thumbprint : context.CertificateThumbprint;
                context.ThumbprintAlgorithm = string.IsNullOrWhiteSpace(context.ThumbprintAlgorithm) ? thumbprintAlgorithm : context.ThumbprintAlgorithm;

                if (!string.IsNullOrWhiteSpace(context.CertificateThumbprint) && string.IsNullOrWhiteSpace(context.ThumbprintAlgorithm))
                {
                    context.ThumbprintAlgorithm = ThumbprintAlgorithmStr;
                }
                else if (string.IsNullOrWhiteSpace(context.CertificateThumbprint) && !string.IsNullOrWhiteSpace(context.ThumbprintAlgorithm))
                {
                    context.ThumbprintAlgorithm = string.Empty;
                }

                var existingExtension = extensionList.Find(e => e.Id == availableId);
                if (existingExtension != null)
                {
                    DeleteExtension(availableId);
                }

                if (r.Default)
                {
                    Cmdlet.WriteVerbose(string.Format(Resources.ServiceExtensionSettingForDefaultRole, context.Type));
                }
                else
                {
                    Cmdlet.WriteVerbose(string.Format(Resources.ServiceExtensionSettingForSpecificRole, context.Type, r.RoleName));
                }

                AddExtension(new HostedServiceExtensionInput
                {
                    Id = availableId,
                    Thumbprint = context.CertificateThumbprint,
                    ThumbprintAlgorithm = context.ThumbprintAlgorithm,
                    ProviderNameSpace = context.ProviderNameSpace,
                    Type = context.Type,
                    PublicConfiguration = context.PublicConfiguration,
                    PrivateConfiguration = context.PrivateConfiguration
                });

                if (r.Default)
                {
                    builder.RemoveDefault(context.ProviderNameSpace, context.Type);
                    builder.AddDefault(availableId);
                }
                else
                {
                    builder.Remove(r.RoleName, context.ProviderNameSpace, context.Type);
                    builder.Add(r.RoleName, availableId);
                }
            }

            return builder.ToConfiguration();
        }

        public void Uninstall(string nameSpace, string type, ExtensionConfiguration extConfig)
        {
            var extBuilder = GetBuilder(extConfig);
            Channel.ListHostedServiceExtensions(SubscriptionId, ServiceName).ForEach(e =>
            {
                if (CheckNameSpaceType(e, nameSpace, type) && !extBuilder.ExistAny(e.Id))
                {
                    Channel.DeleteHostedServiceExtension(SubscriptionId, ServiceName, e.Id);
                }
            });
        }
    }
}
