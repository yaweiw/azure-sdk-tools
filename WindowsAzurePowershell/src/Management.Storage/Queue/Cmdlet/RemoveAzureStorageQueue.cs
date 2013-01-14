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

namespace Microsoft.WindowsAzure.Management.Storage.Queue
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Queue.Contract;

    [Cmdlet(VerbsCommon.Remove, "AzureStorageQueue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High),
        OutputType(typeof(String))]
    public class RemoveAzureStorageQueueCommand : StorageQueueBaseCmdlet
    {
        [Alias("N", "Queue")]
        [Parameter(Position = 0, HelpMessage = "Queue name",
                   ValueFromPipeline = true, Mandatory = true,
                   ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Force to remove the queue without confirm")]
        public SwitchParameter Force
        {
            get { return force; }
            set { force = value; }
        }
        private bool force;

        /// <summary>
        /// Initializes a new instance of the RemoveAzureStorageQueueCommand class.
        /// </summary>
        public RemoveAzureStorageQueueCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureStorageQueueCommand class.
        /// </summary>
        /// <param name="channel">IStorageQueueManagement channel</param>
        public RemoveAzureStorageQueueCommand(IStorageQueueManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// confirm the remove operation
        /// </summary>
        /// <param name="message">confirmation message</param>
        /// <returns>true if user confirm the operation, otherwise false</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual bool ConfirmRemove(string message)
        {
            return ShouldProcess(message);
        }

        /// <summary>
        /// remove an azure queue
        /// </summary>
        /// <param name="name">queue name</param>
        /// <returns>
        /// true if the queue is removed successfully, false if user cancel the remove operation,
        /// otherwise throw an exception
        /// </returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal bool RemoveAzureQueue(string name)
        {
            if (!NameUtil.IsValidQueueName(name))
            {
                throw new ArgumentException(String.Format(Resources.InvalidQueueName, name));
            }

            QueueRequestOptions requestOptions = null;
            CloudQueue queue = Channel.GetQueueReference(name);

            if (!Channel.IsQueueExists(queue, requestOptions, OperationContext))
            {
                throw new ResourceNotFoundException(String.Format(Resources.QueueNotFound, name));
            }

            if (force || ConfirmRemove(name))
            {
                Channel.DeleteQueue(queue, requestOptions, OperationContext);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            String result = string.Empty;

            bool removed = RemoveAzureQueue(Name);

            if (removed)
            {
                result = String.Format(Resources.RemoveQueueSuccessfully, Name);
            }
            else
            {
                result = String.Format(Resources.RemoveQueueCancelled, Name);
            }

            WriteObject(result);
        }
    }
}