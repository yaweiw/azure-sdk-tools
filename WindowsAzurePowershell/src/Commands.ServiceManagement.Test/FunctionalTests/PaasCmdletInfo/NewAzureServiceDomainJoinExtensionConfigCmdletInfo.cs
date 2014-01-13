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
    public class NewAzureServiceDomainJoinExtensionConfigCmdletInfo: CmdletsInfo
    {
        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
        {
            this.cmdletName = Utilities.NewAzureServiceDomainJoinExtensionConfig;
            if(role != null)
            {
                this.cmdletParams.Add(new CmdletParam("Role",role));
            }
            if(!string.IsNullOrEmpty(thumbprintAlgorithm))
            {
                this.cmdletParams.Add(new CmdletParam("ThumbprintAlgorithm",thumbprintAlgorithm));
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

        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string workGroupName,X509Certificate2 certificate,string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(role,thumbprintAlgorithm,restart,credential)
        {
            this.cmdletParams.Add(new CmdletParam("WorkGroupName", workGroupName));
            if (certificate != null)
            {
                this.cmdletParams.Add(new CmdletParam("Certificate", certificate));
            }
        }

        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string workGroupName, string certificateThumbprint = null, string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(role, thumbprintAlgorithm, restart, credential)
        {
            this.cmdletParams.Add(new CmdletParam("WorkGroupName", workGroupName));
            if (!string.IsNullOrEmpty(certificateThumbprint))
            {
                this.cmdletParams.Add(new CmdletParam("CertificateThumbprint", certificateThumbprint));
            }
        }

        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string domainName, string oUPath = null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(role, thumbprintAlgorithm, restart, credential)
        {
            this.cmdletParams.Add(new CmdletParam("DomainName", domainName));
            if (!string.IsNullOrEmpty(oUPath))
            {
                this.cmdletParams.Add(new CmdletParam("OUPath", oUPath));
            }
            if (unjoinDomainCredential != null)
            {
                this.cmdletParams.Add(new CmdletParam("UnjoinDomainCredential", unjoinDomainCredential));
            }
        }

        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string domainName,X509Certificate2 x509Certificate,JoinOptions? options = null,  string oUPath= null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(domainName,oUPath,unjoinDomainCredential,role, thumbprintAlgorithm, restart, credential)
        {
            if (x509Certificate != null)
            {
                this.cmdletParams.Add(new CmdletParam("X509Certificate",x509Certificate));
            }
            if (options.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("Options",options.Value));
            }

        }

        public NewAzureServiceDomainJoinExtensionConfigCmdletInfo(string domainName, string certificateThumbprint, uint? joinOption = null, string oUPath = null, PSCredential unjoinDomainCredential = null,
            string[] role = null, string thumbprintAlgorithm = null, bool restart = false, PSCredential credential = null)
            : this(domainName, oUPath, unjoinDomainCredential, role, thumbprintAlgorithm, restart, credential)
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
    }
}
