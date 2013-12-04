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
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// Show azure storage service properties
    /// </summary>
    [Cmdlet(VerbsCommon.Set, StorageNouns.StorageServiceHourMetrics),
        OutputType(typeof(MetricsProperties))]
    public class SetAzureStorageServiceHourMetricsCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Azure storage type")]
        [ValidateSet(StorageNouns.BlobService, StorageNouns.TableService, StorageNouns.QueueService,
            IgnoreCase = true)]
        public string Type { get; set; }

        [Parameter(HelpMessage = "Metrics version")]
        public double? Version { get; set; }

        [Parameter(HelpMessage = "Metrics retention days. -1 means disable Metrics retention policy, otherwise enable.")]
        [ValidateRange(-1, 365)]
        public int? RetentionDays { get; set; }

        [Parameter(HelpMessage = "Metrics level.(None/Service/ServiceAndApi)")]
        [ValidateSet(StorageNouns.OffMetrics, StorageNouns.MinimalMetrics, StorageNouns.VerboseMetrics,
            IgnoreCase = true)]
        public string MetricsLevel { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Display ServiceProperties")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Update the specified service properties according to the input
        /// </summary>
        /// <param name="serviceProperties">Service properties</param>
        internal void UpdateServiceProperties(MetricsProperties metrics)
        {
            if (Version != null)
            {
                metrics.Version = Version.ToString();
            }

            if (RetentionDays != null)
            {
                if (RetentionDays == -1)
                {
                    //Disable metrics retention policy
                    metrics.RetentionDays = null;
                }
                else if (RetentionDays < 1 || RetentionDays > 365)
                {
                    throw new ArgumentException(string.Format(Resources.InvalidRetentionDay, RetentionDays));
                }
                else
                {
                    metrics.RetentionDays = RetentionDays;
                }
            }

            if (MetricsLevel != null)
            {
                MetricsLevel metricsLevel = GetMetricsLevel(MetricsLevel);
                metrics.MetricsLevel = metricsLevel;
                //Set default metrics version
                if (string.IsNullOrEmpty(metrics.Version))
                {
                    string defaultMetricsVersion = "1.0";
                    metrics.Version = defaultMetricsVersion;
                }
            }
        }

        /// <summary>
        /// Get metrics level
        /// </summary>
        /// <param name="MetricsLevel">The string type of Metrics level</param>
        /// <example>GetMetricsLevel("None"), GetMetricsLevel("Service")</example>
        /// <returns>MetricsLevel object</returns>
        internal MetricsLevel GetMetricsLevel(string MetricsLevel)
        {
            try
            {
                return (MetricsLevel)Enum.Parse(typeof(MetricsLevel), MetricsLevel, true);
            }
            catch 
            {
                throw new ArgumentException(String.Format(Resources.InvalidEnumName, MetricsLevel));
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            ServiceProperties currentServiceProperties = Channel.GetStorageServiceProperties(account,
                Type, GetRequestOptions(Type), OperationContext);
            ServiceProperties serviceProperties = new ServiceProperties();
            SetAzureStorageServiceLoggingCommand.CleanServiceProperties(serviceProperties);
            serviceProperties.HourMetrics = currentServiceProperties.HourMetrics;

            UpdateServiceProperties(serviceProperties.HourMetrics);
            Channel.SetStorageServiceProperties(account, Type, serviceProperties,
                GetRequestOptions(Type), OperationContext);

            if (PassThru)
            {
                WriteObject(serviceProperties.HourMetrics);
            }
        }
    }
}
