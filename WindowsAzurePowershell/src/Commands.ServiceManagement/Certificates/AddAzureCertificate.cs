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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Certificates
{
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using AutoMapper;
    using Commands.Utilities.Common;
    using Helpers;
    using Management.Compute.Models;
    using Management.Compute;

    /// <summary>
    /// Upload a service certificate for the specified hosted service.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureCertificate"), OutputType(typeof(ManagementOperationContext))]
    public class AddAzureCertificate : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Hosted Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "Certificate to deploy.")]
        [ValidateNotNullOrEmpty]
        public object CertToDeploy
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Certificate password.")]
        public string Password
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            Password = Password ?? string.Empty;

            var certData = GetCertificateData();

            var parameters = new ServiceCertificateCreateParameters
            {
                Data = certData,
                Password = Password,
                CertificateFormat = CertificateFormat.Pfx
            };
            ExecuteClientActionNewSM(
                null, 
                CommandRuntime.ToString(), 
                () => this.ComputeClient.ServiceCertificates.Create(this.ServiceName, parameters), 
                (s, r) => ContextFactory<ComputeOperationStatusResponse, ManagementOperationContext>(r, s));
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void OnProcessRecord()
        {
            Mapper.Initialize(m => m.AddProfile<ServiceManagementProfile>());
            this.ExecuteCommand();
        }

        private byte[] GetCertificateData()
        {

            if (((CertToDeploy is PSObject) && ((PSObject)CertToDeploy).ImmediateBaseObject is X509Certificate2) ||
                (CertToDeploy is X509Certificate2))
            {
                var cert = ((PSObject)CertToDeploy).ImmediateBaseObject as X509Certificate2;

                return CertUtils.GetCertificateData(cert);
            }
            else
            {
                var certPath = this.ResolvePath(CertToDeploy.ToString());
                return CertUtils.GetCertificateData(certPath, Password);
            }
        }
    }
}
