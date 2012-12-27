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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using System.Diagnostics;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.Storage.Model;
    using System.Net;

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
                //FIXME need more test for pipeline
                CloudStorageAccount account = GetCurrentStorageAccount();
                StorageContext context = new StorageContext(account);
                Context = context;
                return account;
            }
        }

        internal void SafeWriteObjectWithContext(AzureStorageBase item)
        {
            item.Context = Context;
            WriteOutputObject(item);
        }

        internal CloudStorageAccount GetCurrentStorageAccount()
        {
            SubscriptionData subscription = new SubscriptionData();
            subscription.SubscriptionId = "";
            subscription.CurrentStorageAccount = "";
            //FIXME this only works with sdk 1.8
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

        protected override void ProcessRecord()
        {
            SkipChannelInit = Context != null;

            base.ProcessRecord();
        }
    }
}
