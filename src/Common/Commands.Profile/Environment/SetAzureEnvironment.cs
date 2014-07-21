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

namespace Microsoft.WindowsAzure.Commands.Profile
{
    using Microsoft.WindowsAzure.Commands.Common.Properties;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Utilities.Common;
    /// <summary>
    /// Sets a Microsoft Azure environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureEnvironment"), OutputType(typeof(WindowsAzureEnvironment))]
    public class SetAzureEnvironmentCommand : CmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string PublishSettingsFileUrl { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ServiceEndpoint { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ManagementPortalUrl { get; set; }

        [Parameter(Position = 4, Mandatory = false, HelpMessage = "The storage endpoint")]
        public string StorageEndpoint { get; set; }

        [Parameter(Position = 5, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Active directory endpoint")]
        [Alias("AdEndpointUrl")]
        public string ActiveDirectoryEndpoint { get; set; }

        [Parameter(Position = 6, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud service endpoint")]
        public string ResourceManagerEndpoint { get; set; }

        [Parameter(Position = 7, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The public gallery endpoint")]
        public string GalleryEndpoint { get; set; }

        [Parameter(Position = 8, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            HelpMessage = "Identifier of the target resource that is the recipient of the requested token.")]
        public string ActiveDirectoryServiceEndpointResourceId { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            try
            {
                var env = WindowsAzureProfile.Instance.Environments[Name];
                env.PublishSettingsFileUrl = Value(PublishSettingsFileUrl, env.PublishSettingsFileUrl);
                env.ServiceEndpoint = Value(ServiceEndpoint, env.ServiceEndpoint);
                env.ResourceManagerEndpoint = Value(ResourceManagerEndpoint, env.ResourceManagerEndpoint);
                env.ManagementPortalUrl = Value(ManagementPortalUrl, env.ManagementPortalUrl);
                env.StorageEndpointSuffix = Value(StorageEndpoint, env.StorageEndpointSuffix);
                env.ActiveDirectoryEndpoint = Value(ActiveDirectoryEndpoint, env.ActiveDirectoryEndpoint);
                env.ActiveDirectoryServiceEndpointResourceId = Value(ActiveDirectoryServiceEndpointResourceId, env.ActiveDirectoryServiceEndpointResourceId);
                env.GalleryEndpoint = Value(GalleryEndpoint, env.GalleryEndpoint);

                WindowsAzureProfile.Instance.UpdateEnvironment(env);

                WriteObject(env);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format(Resources.EnvironmentNotFound, Name), ex);
            }
        }

        private static string Value(string newValue, string existingValue)
        {
            return string.IsNullOrEmpty(newValue) ? existingValue : newValue;
        }
    }
}