// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Management.Storage.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    [Cmdlet(VerbsCommon.New, "AzureStorageContext",
        DefaultParameterSetName = "AccountNameKey")]
    public class NewAzureStorageContext : BaseCmdlet
    {
        [Parameter(Position = 0, HelpMessage = "Azure Storage Acccount Name", 
            Mandatory = true, ParameterSetName = "AccountNameKey")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(Position = 1, HelpMessage = "Azure Storage Account Key",
            Mandatory = true, ParameterSetName = "AccountNameKey")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey { get; set; }

        [Alias("sas")]
        [Parameter(HelpMessage = "Azure Storage SAS Token",
            Mandatory = true, ParameterSetName = "AccountSastoken")]
        [ValidateNotNullOrEmpty]
        public string SasToken { get; set; }

        [Alias("conn")]
        [Parameter(HelpMessage = "Azure Storage Connection String",
            Mandatory = true, ParameterSetName = "ConnectionString")]
        [ValidateNotNullOrEmpty]
        public string ConnectionString { get; set; }

        [Parameter(HelpMessage = "Use local development storage account",
            Mandatory = true, ParameterSetName = "LocalDevelopment")]
        public SwitchParameter Local
        {
            get { return isLocalDevAccount; }
            set { isLocalDevAccount = value; }
        }
        private bool isLocalDevAccount;

        [Alias("anon")]
        [Parameter(HelpMessage = "Use local development storage account",
            Mandatory = true, ParameterSetName = "Anonymous")]
        public SwitchParameter Anonymous
        {
            get { return isAnonymous; }
            set { isAnonymous = value; }
        }
        private bool isAnonymous;

        [Parameter(HelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS",
            ParameterSetName = "AccountNameKey")]
        [Parameter(HelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS",
            ParameterSetName = "AccountSastoken")]
        [ValidateSet("http", "https")]
        public string Protocol
        {
            get { return protocolType; }
            set { protocolType = value; }
        }
        private string protocolType = "https";

        public NewAzureStorageContext()
        {
            SkipChannelInit = true;
        }

        internal CloudStorageAccount GetStorageAccountByNameAndKey(string accountName, string accountKey, bool useHttps)
        {
            //FIXME see the implementation of sqldatabase?
            StorageCredentials credential = new StorageCredentials(accountName, accountKey);
            return new CloudStorageAccount(credential, useHttps);
        }

        //FIXME seems cannot work
        internal CloudStorageAccount GetStorageAccountBySasToken(string sasToken, bool useHttps)
        {
            StorageCredentials credential = new StorageCredentials(SasToken);
            return new CloudStorageAccount(credential, useHttps);
        }

        internal CloudStorageAccount GetStorageAccountByConnectionString(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }

        internal CloudStorageAccount GetLocalDevelopmentStorageAccount()
        {
            return CloudStorageAccount.DevelopmentStorageAccount;
        }

        //FIXME seems cannot work
        internal CloudStorageAccount GetAnonymouseStorageAccount()
        {
            StorageCredentials credential = new StorageCredentials();
            return new CloudStorageAccount(credential, false);
        }

        internal override void ExecuteCommand()
        {
            CloudStorageAccount account = null;
            bool useHttps = !("http" == protocolType.ToLower());
            switch (ParameterSetName)
            {
                case "AccountNameKey":
                    account = GetStorageAccountByNameAndKey(StorageAccountName, StorageAccountKey, useHttps);
                    break;
                case "AccountSastoken":
                    account = GetStorageAccountBySasToken(SasToken, useHttps);
                    break;
                case "ConnectionString":
                    account = GetStorageAccountByConnectionString(ConnectionString);
                    break;
                case "LocalDevelopment":
                    account = GetLocalDevelopmentStorageAccount();
                    break;
                case "Anonymous":
                    account = GetAnonymouseStorageAccount();
                    break;
                default:
                    throw new ArgumentException(Resources.InvalidAccountParameterCombination);
            }
            StorageContext context = new StorageContext(account);
            WriteOutputObject(context);
        }
    }
}
