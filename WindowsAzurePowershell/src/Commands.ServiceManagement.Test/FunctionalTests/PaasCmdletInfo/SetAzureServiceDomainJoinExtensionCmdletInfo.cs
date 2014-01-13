
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions;
    using PowershellCore;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;

    public class SetAzureServiceDomainJoinExtensionCmdletInfo : CmdletsInfo
    {
        private enum DomainJoinExtensionParameterSetType
        {
            DomainName,
            WorkGroupName
        }
            

        //Constructor with parameters applicable to all ParameterSets
        private SetAzureServiceDomainJoinExtensionCmdletInfo(DomainJoinExtensionParameterSetType type,string value,
            string[] role = null,  string slot = null,string serviceName = null,string thumbprintAlgorithm = null,bool restart = false,PSCredential credential = null)
        {
            this.cmdletName = Utilities.SetAzureServiceDomainJoinExtension;
            this.cmdletParams.Add(new CmdletParam(type.ToString(), value));
            if (role != null)
            {
                this.cmdletParams.Add(new CmdletParam("Role", role));
            }
            if (!string.IsNullOrEmpty(slot))
            {
                this.cmdletParams.Add(new CmdletParam("Slot", slot));
            }
            if (!string.IsNullOrEmpty(serviceName))
            {
                this.cmdletParams.Add(new CmdletParam("ServiceName",serviceName));
            }
            if (!string.IsNullOrEmpty(thumbprintAlgorithm))
            {
                this.cmdletParams.Add(new CmdletParam("ThumbprintAlgorithm", thumbprintAlgorithm));
            }
            if (restart)
            {
                this.cmdletParams.Add(new CmdletParam("Restart", restart));
            }
            if (credential != null)
            {
                this.cmdletParams.Add(new CmdletParam("Credential", credential));
            }
        }

        //constructors for each parameter set
        
        //BasicDomainParameterSet
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string domainName,PSCredential unjoinDomainCredential = null,
            string[] role = null,  string slot = null,string serviceName = null,string thumbprintAlgorithm = null,bool restart = false,PSCredential credential = null,string oUPath=null)
            : this(DomainJoinExtensionParameterSetType.DomainName, domainName,role,slot,serviceName,thumbprintAlgorithm,restart,credential)
        {
            if (unjoinDomainCredential != null)
            {
                this.cmdletParams.Add(new CmdletParam("UnjoinDomainCredential", unjoinDomainCredential));
            }
            if (!string.IsNullOrEmpty(oUPath))
            {
                this.cmdletParams.Add(new CmdletParam("OUPath",oUPath));
            }
        }

        //DomainJoinParameterSet with X509Certificate2 Certificate
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string domainName,X509Certificate2 x509Certificate,JoinOptions? options =null,PSCredential unjoinDomainCredential = null,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null, string oUPath=null)
            : this(domainName,unjoinDomainCredential,role, slot,serviceName,thumbprintAlgorithm,restart,credential,oUPath)
        {
            if (x509Certificate != null)
            {
                this.cmdletParams.Add(new CmdletParam("X509Certificate2", x509Certificate));
            }
            if (options.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("Options", options.Value));
            }
        }

        //DomainJoinParameterSet with X509Certificate2 Certificate and Join Option number
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string domainName, X509Certificate2 x509Certificate, uint? joinOption = null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null, string oUPath = null)
            : this(domainName, unjoinDomainCredential, role, slot, serviceName, thumbprintAlgorithm, restart, credential, oUPath)
        {
            if (x509Certificate != null)
            {
                this.cmdletParams.Add(new CmdletParam("X509Certificate2", x509Certificate));
            }
            if (!joinOption.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("JoinOption", joinOption.Value));
            }
        }

        //DomainJoinParameterSet with certificate thumbprint 
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string domainName, string certificateThumbprint , JoinOptions? options = null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null, string oUPath = null)
            : this(domainName, unjoinDomainCredential, role, slot, serviceName, thumbprintAlgorithm, restart, credential, oUPath)
        {
            if (!string.IsNullOrEmpty(certificateThumbprint))
            {
                this.cmdletParams.Add(new CmdletParam("CertificateThumbprint", certificateThumbprint));
            }
            if (options.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("Options", options.Value));
            }
        }

        //DomainJoinParameterSet with certificate thumbprint and Join Option number
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string domainName, string certificateThumbprint, uint? joinOption = null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null, string oUPath = null)
            : this(domainName, unjoinDomainCredential, role, slot, serviceName, thumbprintAlgorithm, restart, credential, oUPath)
        {
            if (!string.IsNullOrEmpty(certificateThumbprint))
            {
                this.cmdletParams.Add(new CmdletParam("CertificateThumbprint", certificateThumbprint));
            }
            if (joinOption.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("JoinOption", joinOption.Value));
            }
        }

        //DomainJoinParameterSet with Workgroup name
        public SetAzureServiceDomainJoinExtensionCmdletInfo(string workGroupName, X509Certificate2 x509Certificate,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(DomainJoinExtensionParameterSetType.WorkGroupName, workGroupName, role, slot, serviceName, thumbprintAlgorithm, restart, credential)
        {
            if (x509Certificate != null)
            {
                this.cmdletParams.Add(new CmdletParam("X509Certificate2", x509Certificate));
            }
        }

        public SetAzureServiceDomainJoinExtensionCmdletInfo(string workGroupName, string certificateThumbprint,
            string[] role = null, string slot = null, string serviceName = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(DomainJoinExtensionParameterSetType.WorkGroupName, workGroupName, role, slot, serviceName, thumbprintAlgorithm, restart, credential)
        {
            if (!string.IsNullOrEmpty(certificateThumbprint))
            {
                this.cmdletParams.Add(new CmdletParam("CertificateThumbprint", certificateThumbprint));
            }
        }
    }
}
