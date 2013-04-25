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
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using WindowsAzure.Management.ServiceManagement.Helpers;
    using WindowsAzure.Management.Utilities.CloudService;
    using WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    // From GithubClient.cs
    public static class SecureStringExtensionMethods
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }

    /// <summary>
    /// New Windows Azure Service Remote Desktop Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureServiceRemoteDesktopExtensionConfig"), OutputType(typeof(ExtensionConfigurationContext))]
    public class NewAzureServiceRemoteDesktopExtensionConfigCommand : BaseAzureServiceRemoteDesktopExtensionCmdlet
    {
        public NewAzureServiceRemoteDesktopExtensionConfigCommand()
        {
        }

        public NewAzureServiceRemoteDesktopExtensionConfigCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public string[] Roles
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Remote Desktop Credential")]
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Remote Desktop Credential")]
        public PSCredential Credential
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Remote Desktop User Expiration Date")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public DateTime Expiration
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "ThumbprintAlgorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public string ThumbprintAlgorithm
        {
            get;
            set;
        }

        private string ExpirationStr
        {
            get
            {
                return Expiration.ToString("yyyy-MM-dd");
            }
        }

        protected override bool CheckExtensionType(string extensionId)
        {
            if (!string.IsNullOrEmpty(extensionId))
            {
                HostedServiceExtension ext = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
                return CheckExtensionType(ext);
            }
            return false;
        }

        private bool ValidateParameters()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()), ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                WriteExceptionError(new Exception("Invalid service name"));
                return false;
            }
            else
            {
                ServiceName = serviceName;
            }

            if (!IsServiceAvailable(ServiceName))
            {
                WriteExceptionError(new Exception("Service not found: " + ServiceName));
                return false;
            }

            Expiration = Expiration.Equals(default(DateTime)) ? DateTime.Now.AddMonths(6) : Expiration;

            if (X509Certificate != null)
            {
                var operationDescription = string.Format("{0} - Uploading Certificate: {1}", CommandRuntime, X509Certificate.Thumbprint);
                ExecuteClientActionInOCS(null, operationDescription, s => this.Channel.AddCertificates(s, this.ServiceName, CertUtils.Create(X509Certificate)));
                Thumbprint = X509Certificate.Thumbprint;
                ThumbprintAlgorithm = X509Certificate.SignatureAlgorithm.FriendlyName;
            }
            else if (Thumbprint != null)
            {
                ThumbprintAlgorithm = string.IsNullOrEmpty(ThumbprintAlgorithm) ? "sha1" : ThumbprintAlgorithm;
            }

            return true;
        }

        private void ExecuteCommand()
        {
            WriteObject(new ExtensionConfigurationContext
            {
                Thumbprint = Thumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                PublicConfiguration = string.Format(PublicConfigurationTemplate, Credential.UserName, ExpirationStr),
                PrivateConfiguration = string.Format(PrivateConfigurationTemplate, Credential.Password.ConvertToUnsecureString()),
                AllRoles = Roles == null || Roles.Length <= 0,
                NamedRoles = Roles,
                X509Certificate = X509Certificate
            });
        }

        protected override void OnProcessRecord()
        {
            if (ValidateParameters())
            {
                ExecuteCommand();
            }
            else
            {
                WriteExceptionError(new ArgumentException("Invalid Cmdlet parameters."));
            }
        }
    }
}
