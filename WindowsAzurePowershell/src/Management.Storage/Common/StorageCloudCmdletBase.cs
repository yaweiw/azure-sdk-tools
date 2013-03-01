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

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Threading;
    using ServiceModel = System.ServiceModel;

    /// <summary>
    /// Base cmdlet for all storage cmdlet that works with cloud
    /// </summary>
    public class StorageCloudCmdletBase<T> : CloudBaseCmdlet<T>
        where T : class
    {
        [Parameter(HelpMessage = "Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true)]
        public AzureStorageContext Context {get; set;}

        /// <summary>
        /// Cmdlet operation context.
        /// </summary>
        protected OperationContext OperationContext 
        {
            get
            {
                return CmdletOperationContext.GetStorageOperationContext(WriteVerboseLog);
            }    
        }

        /// <summary>
        /// Write log in verbose mode
        /// </summary>
        /// <param name="msg">Verbose log</param>
        internal void WriteVerboseLog(string msg)
        {
            WriteVerboseWithTimestamp(msg);
        }

        /// <summary>
        /// Get cloud storage account 
        /// </summary>
        /// <returns>Storage account</returns>
        internal CloudStorageAccount GetCloudStorageAccount()
        {
            if (Context != null)
            {
                WriteVerboseLog(String.Format(Resources.UseStorageAccountFromContext, Context.StorageAccountName));
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

                //Set the storage context and use it in pipeline
                Context = new AzureStorageContext(account);

                return account;
            }
        }

        /// <summary>
        /// Output azure storage object with storage context
        /// </summary>
        /// <param name="item">An AzureStorageBase object</param>
        internal void WriteObjectWithStorageContext(AzureStorageBase item)
        {
            item.Context = Context;
            WriteObject(item);
        }

        /// <summary>
        /// Init channel with or without subscription in storage cmdlet
        /// </summary>
        /// <param name="force">Force to create a new channel</param>
        protected override void InitChannelCurrentSubscription(bool force)
        {
            //Create storage management channel
            CreateChannel();
        }

        /// <summary>
        /// Whether should init the service channel or not
        /// </summary>
        /// <returns>True if it need to init the service channel, otherwise false</returns>
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
        /// Output azure storage object with storage context
        /// </summary>
        /// <param name="item">An enumerable collection fo azurestorage object</param>
        internal void WriteObjectWithStorageContext(IEnumerable<AzureStorageBase> itemList)
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
        /// Get current storage account from azure subscription
        /// </summary>
        /// <returns>A storage account</returns>
        private CloudStorageAccount GetStorageAccountFromSubscription()
        {
            string CurrentStorageAccount = CurrentSubscription.CurrentStorageAccount;

            if (string.IsNullOrEmpty(CurrentStorageAccount))
            {
                throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }
            else
            {
                WriteVerboseLog(String.Format(Resources.UseCurrentStorageAccountFromSubscription, CurrentStorageAccount, CurrentSubscription.SubscriptionName));

                try
                {
                    //The service channel initialized by subscription
                    return CurrentSubscription.GetCurrentStorageAccount();
                }
                catch (ServiceModel.CommunicationException e)
                {
                    if (e.IsNotFoundException())
                    {
                        //Repack the 404 error
                        string errorMessage = String.Format(Resources.CurrentStorageAccountNotFoundOnAzure, CurrentStorageAccount);
                        ServiceModel.CommunicationException exception = new ServiceModel.CommunicationException(errorMessage, e);
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
        /// Get storage account from environment variable "AZURE_STORAGE_CONNECTION_STRING"
        /// </summary>
        /// <returns>Cloud storage account</returns>
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

        /// <summary>
        /// Write error details for storageexception
        /// </summary>
        /// <param name="exception">StorageException from storage client</param>
        protected void WriteErrorDetails(StorageException exception)
        {
            exception = exception.RepackStorageException();
            WriteExceptionError(exception);
        }

        /// <summary>
        /// Write error with category and identifier
        /// </summary>
        /// <param name="e">an exception object</param>
        protected override void WriteExceptionError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);

            ErrorCategory errorCategory = ErrorCategory.CloseError; //default error category

            if (e is ArgumentException)
            {
                errorCategory = ErrorCategory.InvalidArgument;
            }
            else if (e is ResourceNotFoundException)
            {
                errorCategory = ErrorCategory.ObjectNotFound;
            }
            else if (e is ResourceAlreadyExistException)
            {
                errorCategory = ErrorCategory.ResourceExists;
            }
            else if (e is StorageException)
            {
                WriteErrorDetails((StorageException)e);
                return;
            }

            WriteError(new ErrorRecord(e, e.GetType().Name, errorCategory, null));
        }

        /// <summary>
        /// Cmdlet begin process
        /// </summary>
        protected override void BeginProcessing()
        {
            CmdletOperationContext.Init();
            WriteVerboseLog(String.Format(Resources.InitOperationContextLog, CmdletOperationContext.ClientRequestId));
            base.BeginProcessing();
        }

        /// <summary>
        /// End processing
        /// </summary>
        protected override void EndProcessing()
        {
            double timespan = CmdletOperationContext.GetRunningMilliseconds();
            string message = string.Format(Resources.EndProcessingLog,
                this.GetType().Name, CmdletOperationContext.StartedRemoteCallCounter, CmdletOperationContext.FinishedRemoteCallCounter, timespan, CmdletOperationContext.ClientRequestId);
            WriteVerboseLog(message);
            base.EndProcessing();
        }
    }
}
