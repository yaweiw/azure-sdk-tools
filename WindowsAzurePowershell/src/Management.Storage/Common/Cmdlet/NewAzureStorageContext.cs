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

namespace Microsoft.WindowsAzure.Management.Storage.Common.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Common.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;
    
    /// <summary>
    /// New storage context
    /// </summary>
    [Cmdlet(VerbsCommon.New, StorageNouns.StorageContext, DefaultParameterSetName = AccountNameKeyParameterSet, SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High),
        OutputType(typeof(AzureStorageContext))]
    public class NewAzureStorageContext : CmdletBase
    {
        /// <summary>
        /// Default parameter set name
        /// </summary>
        private const string AccountNameKeyParameterSet = "AccountNameAndKey";

        /// <summary>
        /// Sas token parameter set name
        /// </summary>
        private const string SasTokenParameterSet = "SasToken";

        /// <summary>
        /// Connection string parameter set name
        /// </summary>
        private const string ConnectionStringParameterSet = "ConnectionString";

        /// <summary>
        /// Local development account parameter set name
        /// </summary>
        private const string LocalParameterSet = "LocalDevelopment";

        /// <summary>
        /// Anonymous storage account parameter set name
        /// </summary>
        private const string AnonymousParameterSet = "AnonymousAccount";

        [Parameter(Position = 0, HelpMessage = "Azure Storage Acccount Name",
            Mandatory = true, ParameterSetName = AccountNameKeyParameterSet)]
        [Parameter(Position = 0, HelpMessage = "Azure Storage Acccount Name",
            Mandatory = true, ParameterSetName = AnonymousParameterSet)]
        [Parameter(Position = 0, HelpMessage = "Azure Storage Acccount Name",
            Mandatory = true, ParameterSetName = SasTokenParameterSet)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(Position = 1, HelpMessage = "Azure Storage Account Key",
            Mandatory = true, ParameterSetName = AccountNameKeyParameterSet)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey { get; set; }

        [Alias("sas")]
        [Parameter(HelpMessage = "Azure Storage SAS Token",
            Mandatory = true, ParameterSetName = SasTokenParameterSet)]
        [ValidateNotNullOrEmpty]
        public string SasToken { get; set; }

        [Alias("conn")]
        [Parameter(HelpMessage = "Azure Storage Connection String",
            Mandatory = true, ParameterSetName = ConnectionStringParameterSet)]
        [ValidateNotNullOrEmpty]
        public string ConnectionString { get; set; }

        [Parameter(HelpMessage = "Use local development storage account",
            Mandatory = true, ParameterSetName = LocalParameterSet)]
        public SwitchParameter Local
        {
            get { return isLocalDevAccount; }
            set { isLocalDevAccount = value; }
        }
        private bool isLocalDevAccount;

        [Alias("anon")]
        [Parameter(HelpMessage = "Use anonymous storage account",
            Mandatory = true, ParameterSetName = AnonymousParameterSet)]
        public SwitchParameter Anonymous
        {
            get { return isAnonymous; }
            set { isAnonymous = value; }
        }
        private bool isAnonymous;

        [Parameter(HelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS",
            ParameterSetName = AccountNameKeyParameterSet)]
        [Parameter(HelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS",
            ParameterSetName = SasTokenParameterSet)]
        [Parameter(HelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS",
            ParameterSetName = AnonymousParameterSet)]
        [ValidateSet(StorageNouns.HTTP, StorageNouns.HTTPS, IgnoreCase = true)]
        public string Protocol
        {
            get { return protocolType; }
            set { protocolType = value; }
        }
        private string protocolType = StorageNouns.HTTPS;

        /// <summary>
        /// Get storage account by account name and account key
        /// </summary>
        /// <param name="accountName">Storage account name</param>
        /// <param name="accountKey">Storage account key</param>
        /// <param name="useHttps">Use https or not</param>
        /// <returns>A storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetStorageAccountByNameAndKey(string accountName, string accountKey, bool useHttps)
        {
            StorageCredentials credential = new StorageCredentials(accountName, accountKey);
            return new CloudStorageAccount(credential, useHttps);
        }

        /// <summary>
        /// Get storage account by sastoken
        /// </summary>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <param name="sasToken">Sas token</param>
        /// <param name="useHttps">Use https or not</param>
        /// <returns>a storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetStorageAccountBySasToken(string storageAccountName, string sasToken, bool useHttps)
        {
            StorageCredentials credential = new StorageCredentials(sasToken);
            return GetStorageAccountWithEndPoint(credential, storageAccountName, useHttps);
        }

        /// <summary>
        /// Get storage account by connection string
        /// </summary>
        /// <param name="connectionString">Azure storage connection string</param>
        /// <returns>A storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetStorageAccountByConnectionString(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }

        /// <summary>
        /// Get local development storage account
        /// </summary>
        /// <returns>A storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetLocalDevelopmentStorageAccount()
        {
            return CloudStorageAccount.DevelopmentStorageAccount;
        }

        /// <summary>
        /// Get anonymous storage account
        /// </summary>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <returns>A storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetAnonymousStorageAccount(string storageAccountName, bool useHttps)
        {
            StorageCredentials credential = new StorageCredentials();
            return GetStorageAccountWithEndPoint(credential, storageAccountName, useHttps);
        }

        /// <summary>
        /// Get storage account and use specific end point
        /// </summary>
        /// <param name="credential">Storage credentail</param>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <returns>A storage account</returns>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal CloudStorageAccount GetStorageAccountWithEndPoint(StorageCredentials credential, string storageAccountName, bool useHttps)
        {
            if (String.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, StorageNouns.StorageAccountName));
            }

            string blobEndPoint = string.Empty;
            string tableEndPoint = string.Empty;
            string queueEndPoint = string.Empty;
            if (useHttps)
            {
                blobEndPoint = String.Format(Resources.HttpsBlobEndPointFormat, storageAccountName);
                tableEndPoint = String.Format(Resources.HttpsTableEndPointFormat, storageAccountName);
                queueEndPoint = String.Format(Resources.HttpsQueueEndPointFormat, storageAccountName);
            }
            else
            {
                blobEndPoint = String.Format(Resources.HttpBlobEndPointFormat, storageAccountName);
                tableEndPoint = String.Format(Resources.HttpTableEndPointFormat, storageAccountName);
                queueEndPoint = String.Format(Resources.HttpQueueEndPointFormat, storageAccountName);
            }
            return new CloudStorageAccount(credential, new Uri(blobEndPoint), new Uri(tableEndPoint), new Uri(queueEndPoint));
        }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            CloudStorageAccount account = null;
            bool useHttps = (StorageNouns.HTTPS.ToLower() == protocolType.ToLower());

            switch (ParameterSetName)
            {
                case AccountNameKeyParameterSet:
                    account = GetStorageAccountByNameAndKey(StorageAccountName, StorageAccountKey, useHttps);
                    break;
                case SasTokenParameterSet:
                    account = GetStorageAccountBySasToken(StorageAccountName, SasToken, useHttps);
                    break;
                case ConnectionStringParameterSet:
                    account = GetStorageAccountByConnectionString(ConnectionString);
                    break;
                case LocalParameterSet:
                    account = GetLocalDevelopmentStorageAccount();
                    break;
                case AnonymousParameterSet:
                    account = GetAnonymousStorageAccount(StorageAccountName, useHttps);
                    break;
                default:
                    throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }

            AzureStorageContext context = new AzureStorageContext(account);
            WriteObject(context);
        }
    }
}
