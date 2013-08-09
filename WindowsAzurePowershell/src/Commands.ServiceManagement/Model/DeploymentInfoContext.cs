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
    using WindowsAzure.ServiceManagement;

    public class DeploymentInfoContext : ServiceOperationContext
    {
        private readonly XNamespace ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";

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
            this.DnsSettings = deployment.Dns;

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
                this.RoleInstanceList = new List<RoleInstance>();
                foreach (var roleInstance in deployment.RoleInstanceList)
                {
                    this.RoleInstanceList.Add(roleInstance);
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

        public IList<RoleInstance> RoleInstanceList
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

        public DnsSettings DnsSettings
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
