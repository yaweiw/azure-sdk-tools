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


namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using AutoMapper;
    using Management.Compute.Models;
    using PersistentVMModel;
    using WindowsAzure.ServiceManagement;

    public class DeploymentInfoContext : ServiceOperationContext
    {
        private readonly XNamespace ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";

        public DeploymentInfoContext(DeploymentGetResponse deployment)
        {
            this.Slot = deployment.DeploymentSlot.ToString();
            this.Name = deployment.Name;
            this.DeploymentName = deployment.Name;
            this.Url = deployment.Uri;
            this.Status = deployment.Status.ToString();
            this.DeploymentId = deployment.PrivateId;
            this.VNetName = deployment.VirtualNetworkName;
            this.SdkVersion = deployment.SdkVersion;

            if (deployment.DnsSettings != null)
            {
                this.DnsSettings = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsSettings
                {
                    DnsServers = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServerList()
                };

                foreach (var dns in deployment.DnsSettings.DnsServers)
                {
                    var newDns = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServer
                    {
                        Name = dns.Name,
                        Address = dns.Address.ToString()
                    };
                    this.DnsSettings.DnsServers.Add(newDns);
                }
            }

            bool result = false;
            bool.TryParse(deployment.RollbackAllowed, out result);
            this.RollbackAllowed = result;


            if (deployment.UpgradeStatus != null)
            {
                this.CurrentUpgradeDomain = deployment.UpgradeStatus.CurrentUpgradeDomain;
                this.CurrentUpgradeDomainState = deployment.UpgradeStatus.CurrentUpgradeDomainState.ToString();
                this.UpgradeType = deployment.UpgradeStatus.UpgradeType.ToString();
            }

            this.Configuration = string.IsNullOrEmpty(deployment.Configuration)
                                     ? string.Empty
                                     : deployment.Configuration;

            this.Label = string.IsNullOrEmpty(deployment.Label)
                             ? string.Empty
                             : deployment.Label;

            if (deployment.RoleInstances != null)
            {
                this.RoleInstanceList = new List<Microsoft.WindowsAzure.Management.Compute.Models.RoleInstance>();
                foreach (var roleInstance in deployment.RoleInstances)
                {
                    var newRoleInstance = new Microsoft.WindowsAzure.Management.Compute.Models.RoleInstance();
                    this.RoleInstanceList.Add(Mapper.Map(roleInstance, newRoleInstance));
                }
            }

            if (!string.IsNullOrEmpty(deployment.Configuration))
            {
                string xmlString = this.Configuration;

                XDocument doc;
                using (var stringReader = new StringReader(xmlString))
                {
                    XmlReader reader = XmlReader.Create(stringReader);
                    doc = XDocument.Load(reader);
                }

                this.OSVersion = doc.Root.Attribute("osVersion") != null ?
                                 doc.Root.Attribute("osVersion").Value :
                                 string.Empty;

                this.RolesConfiguration = new Dictionary<string, RoleConfiguration>();

                var roles = doc.Root.Descendants(this.ns + "Role");

                foreach (var role in roles)
                {
                    this.RolesConfiguration.Add(role.Attribute("name").Value, new RoleConfiguration(role));
                }
            }
        }

        public DeploymentInfoContext(Deployment deployment)
        {
            this.Slot = deployment.DeploymentSlot;
            this.Name = deployment.Name;
            this.DeploymentName = deployment.Name;
            this.Url = deployment.Url;
            this.Status = deployment.Status;
            this.DeploymentId = deployment.PrivateID;
            this.VNetName = deployment.VirtualNetworkName;
            this.SdkVersion = deployment.SdkVersion;

            if (deployment.Dns != null)
            {
                this.DnsSettings = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsSettings
                {
                    DnsServers = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServerList()
                };

                foreach (var dns in deployment.Dns.DnsServers)
                {
                    var newDns = new Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsServer
                    {
                        Name = dns.Name,
                        Address = dns.Address.ToString()
                    };
                    this.DnsSettings.DnsServers.Add(newDns);
                }
            }

            if (deployment.RollbackAllowed.HasValue)
            {
                this.RollbackAllowed = deployment.RollbackAllowed;
            }

            if (deployment.UpgradeStatus != null)
            {
                this.CurrentUpgradeDomain = deployment.UpgradeStatus.CurrentUpgradeDomain;
                this.CurrentUpgradeDomainState = deployment.UpgradeStatus.CurrentUpgradeDomainState;
                this.UpgradeType = deployment.UpgradeStatus.UpgradeType;
            }

            this.Configuration = string.IsNullOrEmpty(deployment.Configuration)
                                     ? string.Empty
                                     : deployment.Configuration;

            this.Label = string.IsNullOrEmpty(deployment.Label)
                             ? string.Empty
                             : deployment.Label;

            if (deployment.RoleInstanceList != null)
            {
                this.RoleInstanceList = new List<Microsoft.WindowsAzure.Management.Compute.Models.RoleInstance>();
                foreach (var roleInstance in deployment.RoleInstanceList)
                {
                    var newRoleInstance = new Microsoft.WindowsAzure.Management.Compute.Models.RoleInstance();
                    this.RoleInstanceList.Add(Mapper.Map(roleInstance, newRoleInstance));
                }
            }

            if (!string.IsNullOrEmpty(deployment.Configuration))
            {
                string xmlString = this.Configuration;

                XDocument doc;
                using (var stringReader = new StringReader(xmlString))
                {
                    XmlReader reader = XmlReader.Create(stringReader);
                    doc = XDocument.Load(reader);
                }

                this.OSVersion = doc.Root.Attribute("osVersion") != null ?
                                 doc.Root.Attribute("osVersion").Value : 
                                 string.Empty;

                this.RolesConfiguration = new Dictionary<string, RoleConfiguration>();

                var roles = doc.Root.Descendants(this.ns + "Role");

                foreach (var role in roles)
                {
                    this.RolesConfiguration.Add(role.Attribute("name").Value, new RoleConfiguration(role));
                }
            }
        }

        public string SdkVersion
        {
            get;
            protected set; 
        }

        public bool? RollbackAllowed
        {
            get;
            protected set; 
        }

        public string Slot
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public string DeploymentName
        {
            get;
            protected set;
        }

        public Uri Url
        {
            get;
            protected set;
        }

        public string Status
        {
            get;
            protected set;
        }

        public int CurrentUpgradeDomain 
        { 
            get; 
            set; 
        } 
        
        public string CurrentUpgradeDomainState 
        { 
            get; 
            set; 
        }
        
        public string UpgradeType 
        { 
            get; 
            set; 
        } 

        public IList<Microsoft.WindowsAzure.Management.Compute.Models.RoleInstance> RoleInstanceList
        {
            get;
            protected set;
        }

        public string Configuration
        {
            get;
            protected set;
        }

        public string DeploymentId
        {
            get;
            protected set;
        }

        public string Label
        {
            get;
            protected set;
        }

        public string VNetName
        {
            get;
            protected set;
        }

        public Microsoft.WindowsAzure.Commands.ServiceManagement.Model.PersistentVMModel.DnsSettings DnsSettings
        {
            get;
            protected set;
        }

        public string OSVersion { get; set; }

        public IDictionary<string, RoleConfiguration> RolesConfiguration
        {
            get;
            protected set;
        }

        public XDocument SerializeRolesConfiguration()
        {
            XDocument document = new XDocument();

            XElement rootElement = new XElement(this.ns + "ServiceConfiguration");
            document.Add(rootElement);

            rootElement.SetAttributeValue("serviceName", this.ServiceName);
            rootElement.SetAttributeValue("osVersion", this.OSVersion);
            rootElement.SetAttributeValue("xmlns", this.ns.ToString());

            foreach (var roleConfig in this.RolesConfiguration)
            {
                rootElement.Add(roleConfig.Value.Serialize());                
            }

            return document;
        }
    }
}
