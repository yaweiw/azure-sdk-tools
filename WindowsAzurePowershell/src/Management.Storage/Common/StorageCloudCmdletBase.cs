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
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel.Storage;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.Service;
    using Microsoft.WindowsAzure.Management.Utilities;
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
    using System.ServiceModel;
    using System.Text;

    /// <summary>
    /// base cmdlet for all storage cmdlet that works with cloud
    /// </summary>
    public class StorageCloudCmdletBase : StorageCmdletBase
    {
        [Parameter(HelpMessage = "Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true)]
        public AzureStorageContext Context {get; set;}

        /// <summary>
        /// get cloud storage account 
        /// </summary>
        /// <returns>storage account</returns>
        internal CloudStorageAccount GetCloudStorageAccount()
        {
            if (Context != null)
            {
                return Context.StorageAccount;
            }
            else
            {
                CloudStorageAccount account = null;

                if (ShouldInitServiceChannel())
                {
                    account = GetStorageAccountFromSubscription();
                }
                else
                {
                    account = GetStorageAccountFromEnvironmentVariable();
                }

                //set the storage context and use it in pipeline
                AzureStorageContext context = new AzureStorageContext(account);
                Context = context;

                return account;
            }
        }

        /// <summary>
        /// output azure storage object with storage context
        /// </summary>
        /// <param name="item">an AzureStorageBase object</param>
        internal void WriteObjectWithStorageContext(AzureStorageBase item)
        {
            item.Context = Context;
            WriteObject(item);
        }

        /// <summary>
        /// init channel with or without subscription in storage cmdlet
        /// </summary>
        /// <param name="force">force to create a new channel</param>
        protected override void InitChannelCurrentSubscription(bool force)
        {
            if (ShouldInitServiceChannel())
            {
                //force storagecmdletbase to create a service management channel
                base.InitChannelCurrentSubscription(true);
            }
            else
            {
                //only create the storage client.
                CreateChannel();
            }
        }

        /// <summary>
        /// whether should init the service channel or not
        /// </summary>
        /// <returns>true if it need to init the service channel, otherwise false</returns>
        internal virtual bool ShouldInitServiceChannel()
        {
            //Storage Context is empty and have already set the current storage account in subscription
            if (Context == null && CurrentSubscription != null &&
                !String.IsNullOrEmpty(CurrentSubscription.CurrentStorageAccount))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// output azure storage object with storage context
        /// </summary>
        /// <param name="item">an eunmerable collection fo azurestorage object</param>
        internal void WriteObjectWithStorageContext(IEnumerable<AzureStorageBase>  itemList)
        {
            if (null == itemList)
            {
                return;
            }

            foreach (AzureStorageBase item in itemList)
            {
                WriteObjectWithStorageContext(item);
            }
        }

        /// <summary>
        /// get current storage account from azure subscription
        /// </summary>
        /// <returns>a storage account</returns>
        private CloudStorageAccount GetStorageAccountFromSubscription()
        {
            string CurrentStorageAccount = CurrentSubscription.CurrentStorageAccount;
            if (string.IsNullOrEmpty(CurrentStorageAccount))
            {
                throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }
            else
            {
                //the service channel have already initialized
                WriteVerboseLog(String.Format(Resources.UseCurrentStorageAccountFromSubscription, CurrentStorageAccount, CurrentSubscription.SubscriptionName));

                try
                {
                    return CurrentSubscription.GetCurrentStorageAccount(Channel);
                }
                catch (CommunicationException e)
                {
                    if (e.IsNotFoundException())
                    {
                        //repack the 404 error
                        string errorMessage = String.Format(Resources.CurrentStorageAccountNotFoundOnAzure, CurrentStorageAccount);
                        CommunicationException exception = new CommunicationException(errorMessage, e);
                        throw exception;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// get storage account from environment variable "AZURE_STORAGE_CONNECTION_STRING"
        /// </summary>
        /// <returns>cloud storage account</returns>
        private CloudStorageAccount GetStorageAccountFromEnvironmentVariable()
        {
            String connectionString = System.Environment.GetEnvironmentVariable(Resources.EnvConnectionString);

            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }
            else
            {
                WriteVerboseLog(Resources.GetStorageAccountFromEnvironmentVariable);
                return CloudStorageAccount.Parse(connectionString);
            }
        }
    }
}
