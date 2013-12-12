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

namespace Microsoft.WindowsAzure.Commands.Storage.Common.Cmdlet
{
    using System.Globalization;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// Show azure storage service properties
    /// </summary>
    [Cmdlet(VerbsCommon.Get, StorageNouns.StorageServiceMetrics),
        OutputType(typeof(MetricsProperties))]
    public class GetAzureStorageServiceMetricsCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = GetAzureStorageServiceLoggingCommand.ServiceTypeHelpMessage)]
        [ValidateSet(StorageNouns.BlobService, StorageNouns.TableService, StorageNouns.QueueService, IgnoreCase = true)]
        public string ServiceType { get; set; }

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Azure storage service metrics type(Hour, Minute).")]
        [ValidateSet(StorageNouns.MetricsType.Hour, StorageNouns.MetricsType.Minute, IgnoreCase = true)]
        public string MetricsType { get; set; }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            ServiceProperties serviceProperties = Channel.GetStorageServiceProperties(account,
                ServiceType, GetRequestOptions(ServiceType) , OperationContext);

            switch (CultureInfo.CurrentCulture.TextInfo.ToTitleCase(MetricsType))
            {
                case StorageNouns.MetricsType.Hour:
                    WriteObject(serviceProperties.HourMetrics);
                    break;
                case StorageNouns.MetricsType.Minute:
                default:
                    WriteObject(serviceProperties.MinuteMetrics);
                    break;
            }
        }
    }
}
