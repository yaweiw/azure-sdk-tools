﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;

    /// <summary>
    /// remove specified azure container
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, StorageNouns.Container, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High),
        OutputType(typeof(String))]
    public class RemoveAzureStorageContainerCommand : StorageCloudBlobCmdletBase
    {
        [Alias("N", "Container")]
        [Parameter(Position = 0, Mandatory = true,
            HelpMessage = "Container Name",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Force to remove the container without confirm")]
        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }
        private bool force;

        /// <summary>
        /// Initializes a new instance of the RemoveAzureStorageContainerCommand class.
        /// </summary>
        public RemoveAzureStorageContainerCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureStorageContainerCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public RemoveAzureStorageContainerCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// confirm the remove operation
        /// </summary>
        /// <param name="message">confirmation message</param>
        /// <returns>true if the operation is confirmed by user, otherwise false</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual bool ConfirmRemove(string message)
        {
            return ShouldProcess(message);
        }

        /// <summary>
        /// remove azure container by container name
        /// </summary>
        /// <param name="name">container name</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal void RemoveAzureContainer(string name)
        {
            if (!NameUtil.IsValidContainerName(name))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, name));
            }

            BlobRequestOptions requestOptions = null;
            AccessCondition accessCondition = null;

            CloudBlobContainer container = Channel.GetContainerReference(name);

            if (!Channel.DoesContainerExist(container, requestOptions, OperationContext))
            {
                throw new ResourceNotFoundException(String.Format(Resources.ContainerNotFound, name));
            }

            string result = string.Empty;

            if (force || ConfirmRemove(name))
            {
                Channel.DeleteContainer(container, accessCondition, requestOptions, OperationContext);
                result = String.Format(Resources.RemoveContainerSuccessfully, name);
            }
            else
            {
                result = String.Format(Resources.RemoveContainerCancelled, name);
            }

            WriteObject(result);
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            RemoveAzureContainer(Name);
        }
    }
}
