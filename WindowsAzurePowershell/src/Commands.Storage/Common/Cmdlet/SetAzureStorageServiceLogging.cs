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
    using System.Globalization;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using StorageClient = WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// Show azure storage service properties
    /// </summary>
    [Cmdlet(VerbsCommon.Set, StorageNouns.StorageServiceLogging),
        OutputType(typeof(LoggingProperties))]
    public class SetAzureStorageServiceLoggingCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Azure storage type")]
        [ValidateSet(StorageNouns.BlobService, StorageNouns.TableService, StorageNouns.QueueService,
            IgnoreCase = true)]
        public string Type { get; set; }

        [Parameter(HelpMessage = "Logging version")]
        public double? Version { get; set; }

        [Parameter(HelpMessage = "Logging retention days. -1 means disable Logging retention policy, otherwise enable.")]
        [ValidateRange(-1, 365)]
        public int? RetentionDays { get; set; }

        public const string LoggingOperationHelpMessage =
            "Logging operations. (All, None, combinations of Read, Write, delete that are seperated by semicolon.)";
        [Parameter(HelpMessage = LoggingOperationHelpMessage)]
        public string LoggingOperations { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Display ServiceProperties")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Update the specified service properties according to the input
        /// </summary>
        /// <param name="logging">Service properties</param>
        internal void UpdateServiceProperties(LoggingProperties logging)
        {
            if (Version != null)
            {
                logging.Version = Version.ToString();
            }

            if (RetentionDays != null)
            {
                if (RetentionDays == -1)
                {
                    //Disable logging retention policy
                    logging.RetentionDays = null;
                }
                else if (RetentionDays < 1 || RetentionDays > 365)
                {
                    throw new ArgumentException(string.Format(Resources.InvalidRetentionDay, RetentionDays));
                }
                else
                {
                    logging.RetentionDays = RetentionDays;
                }
            }

            if (LoggingOperations != null)
            {
                LoggingOperations logOperations = GetLoggingOperations(LoggingOperations);
                logging.LoggingOperations = logOperations;
                //Set default logging version
                if (string.IsNullOrEmpty(logging.Version))
                {
                    string defaultLoggingVersion = "1.0";
                    logging.Version = defaultLoggingVersion;
                }
            }
        }

        /// <summary>
        /// Get logging operations
        /// </summary>
        /// <param name="LoggingOperations">The string type of Logging operations</param>
        /// <example>GetLoggingOperations("all"), GetLoggingOperations("read, write")</example>
        /// <returns>LoggingOperations object</returns>
        internal LoggingOperations GetLoggingOperations(string LoggingOperations)
        {
            LoggingOperations = LoggingOperations.ToLower();
            if (LoggingOperations.IndexOf("all") != -1)
            {
                if (LoggingOperations == "all")
                {
                    return StorageClient.LoggingOperations.All;
                }
                else
                {
                    throw new ArgumentException(LoggingOperationHelpMessage);
                }
            }
            else if (LoggingOperations.IndexOf("none") != -1)
            {
                if (LoggingOperations == "none")
                {
                    return StorageClient.LoggingOperations.None;
                }
                else
                {
                    throw new ArgumentException(LoggingOperationHelpMessage);
                }
            }
            else 
            {
                try
                {
                    return (StorageClient.LoggingOperations)Enum.Parse(typeof(StorageClient.LoggingOperations),
                        LoggingOperations, true);
                }
                catch
                {
                    throw new ArgumentException(String.Format(Resources.InvalidEnumName, LoggingOperations));
                }
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
            serviceProperties.Logging = currentServiceProperties.Logging;

            UpdateServiceProperties(serviceProperties.Logging);

            Channel.SetStorageServiceProperties(account, Type, serviceProperties,
                GetRequestOptions(Type), OperationContext);

            if (PassThru)
            {
                WriteObject(serviceProperties.Logging);
            }
        }
    }
}
