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
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.Storage.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.Text;

    /// <summary>
    /// base cmdlet for all storage cmdlet
    /// </summary>
    public class StorageBaseCmdlet : BaseCmdlet
    {
        [Parameter(HelpMessage = "Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true)]
        public StorageContext Context {get; set;}

        /// <summary>
        /// get cloud storage account 
        /// </summary>
        /// <returns>sotrage account</returns>
        internal CloudStorageAccount GetCloudStorageAccount()
        {
            if (Context != null)
            {
                return Context.StorageAccount;
            }
            else
            {
                CloudStorageAccount account = GetCurrentStorageAccount();
                StorageContext context = new StorageContext(account);
                Context = context;
                return account;
            }
        }

        /// <summary>
        /// output azure storage object with storage context
        /// </summary>
        /// <param name="item"></param>
        internal void SafeWriteObjectWithContext(AzureStorageBase item)
        {
            item.Context = Context;
            WriteOutputObject(item);
        }

        /// <summary>
        /// get current storage account from azure subscription
        /// </summary>
        /// <returns></returns>
        internal CloudStorageAccount GetCurrentStorageAccount()
        {
            //----------------------------------------------------
            //
            // Since azure powershell cannot works with storage client 2.0, 
            // it implemented using enviornment variables in order to not block testers' work. 
            // When azure powershell upgrade, So do this interface.
            //
            //----------------------------------------------------
            SubscriptionData subscription = new SubscriptionData();
            subscription.SubscriptionId = "";
            subscription.CurrentStorageAccount = "";
            //FIXME this only works with sdk 1.8, and the mangemange project should be upgraded
            //subscription.GetCurrentStorageAccount(Channel);
            //throw new NotImplementedException("Current Azure Subscription can not work with sdk 2.0, please use new-azurestoragecontext");
            string envAccountName = "AZURE_STORAGE_ACCOUNT";
            string envAccountKey = "AZURE_STORAGE_ACCESS_KEY";
            String accountName = System.Environment.GetEnvironmentVariable(envAccountName);
            String accountKey = System.Environment.GetEnvironmentVariable(envAccountKey);
            if (String.IsNullOrEmpty(accountName) || String.IsNullOrEmpty(accountKey))
            {
                throw new ArgumentException(Resources.StorageCredentialsNotFound);
            }
            else
            {
                StorageCredentials credential = new StorageCredentials(accountName, accountKey);
                return new CloudStorageAccount(credential, true);
            }
        }

        /// <summary>
        /// process record
        /// </summary>
        protected override void ProcessRecord()
        {
            SkipChannelInit = Context != null;

            base.ProcessRecord();
        }
    }
}
