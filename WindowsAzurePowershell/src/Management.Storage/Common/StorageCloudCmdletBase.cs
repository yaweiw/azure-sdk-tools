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
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Common.ResourceModel;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Util;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.ServiceModel;

    /// <summary>
    /// base cmdlet for all storage cmdlet that works with cloud
    /// </summary>
    public class StorageCloudCmdletBase<T> : CloudBaseCmdlet<T>
        where T : class
    {
        [Parameter(HelpMessage = "Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true)]
        public AzureStorageContext Context {get; set;}

        /// <summary>
        /// cmdlet operation context.
        /// </summary>
        protected Microsoft.WindowsAzure.Storage.OperationContext OperationContext { get; private set; }

        /// <summary>
        /// remote call counter
        /// </summary>
        private int remoteCallCounter = 0;

        /// <summary>
        /// whether stop processing
        /// </summary>
        protected bool ShouldForceQuit = false;

        /// <summary>
        /// init storage client operation context
        /// </summary>
        internal void InitOperationContext()
        {
            OperationContext = new Microsoft.WindowsAzure.Storage.OperationContext();
            OperationContext.Init();

            OperationContext.SendingRequest += (s, e) =>
            {
                try
                {
                    remoteCallCounter++;
                    string message = String.Format(Resources.StartRemoteCall,
                        remoteCallCounter, e.Request.Method, e.Request.RequestUri.ToString());
                    WriteDebugLog(message);
                }
                catch
                {
                    //catch the exception to forbidden storage client to sleep
                }
            };

            OperationContext.ResponseReceived += (s, e) =>
            {
                try
                {
                    string message = String.Format(Resources.FinishRemoteCall,
                        e.Response.StatusCode, e.RequestInformation.ServiceRequestID);
                    WriteDebugLog(message);
                }
                catch
                {
                    //catch the exception to forbidden storage client to sleep
                }
            };

            WriteVerboseWithTimestamp(String.Format(Resources.InitOperationContextLog, this.GetType().Name, OperationContext.ClientRequestID));
        }

        /// <summary>
        /// write log in verbose mode
        /// </summary>
        /// <param name="msg">verbose log</param>
        internal void WriteDebugLog(string msg)
        {
            WriteDebugWithTimestamp(msg);
        }

        /// <summary>
        /// get cloud storage account 
        /// </summary>
        /// <returns>storage account</returns>
        internal CloudStorageAccount GetCloudStorageAccount()
        {
            if (Context != null)
            {
                WriteDebugLog(String.Format(Resources.UseStorageAccountFromContext, Context.StorageAccountName));
                return Context.StorageAccount;
            }
            else
            {
                CloudStorageAccount account = null;
                bool shouldInitChannel = ShouldInitServiceChannel();

                try
                {
                    if (shouldInitChannel)
                    {
                        account = GetStorageAccountFromSubscription();
                    }
                    else
                    {
                        account = GetStorageAccountFromEnvironmentVariable();
                    }
                }
                catch (Exception e)
                {
                    //stop the pipeline if storage account is missed.
                    WriteTerminatingError(e);
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
            //create storage management channel
            CreateChannel();
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
                WriteDebugLog(String.Format(Resources.UseCurrentStorageAccountFromSubscription, CurrentStorageAccount, CurrentSubscription.SubscriptionName));

                try
                {
                    //the service channel initialized by subscription
                    return CurrentSubscription.GetCurrentStorageAccount();
                }
                catch (CommunicationException e)
                {
                    WriteVerboseWithTimestamp(Resources.CannotGetSotrageAccountFromSubscription);

                    if (e.IsNotFoundException())
                    {
                        //repack the 404 error
                        string errorMessage = String.Format(Resources.CurrentStorageAccountNotFoundOnAzure, CurrentStorageAccount, CurrentSubscription.SubscriptionName);
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
                WriteVerboseWithTimestamp(Resources.GetStorageAccountFromEnvironmentVariable);

                try
                {
                    return CloudStorageAccount.Parse(connectionString);
                }
                catch
                {
                    WriteVerboseWithTimestamp(Resources.CannotGetStorageAccountFromEnvironmentVariable);
                    throw;
                }
            }
        }

        /// <summary>
        /// write error with category and identifier
        /// </summary>
        /// <param name="e">an exception object</param>
        protected override void WriteExceptionError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);
            
            if (e is StorageException)
            {
                e = ((StorageException) e).RepackStorageException();
            }
            
            WriteError(new ErrorRecord(e, e.GetType().Name, GetExceptionErrorCategory(e), null));
        }

        /// <summary>
        /// get the error category for specificed exception
        /// </summary>
        /// <param name="e">exception object</param>
        /// <returns>error category</returns>
        protected ErrorCategory GetExceptionErrorCategory(Exception e)
        {
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

            return errorCategory;
        }

        /// <summary>
        /// write terminating error
        /// </summary>
        /// <param name="e">exception object</param>
        protected void WriteTerminatingError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);
            ThrowTerminatingError(new ErrorRecord(e, e.GetType().Name, GetExceptionErrorCategory(e), null));
        }

        /// <summary>
        /// cmdlet begin process
        /// </summary>
        protected override void BeginProcessing()
        {
            InitOperationContext();

            base.BeginProcessing();
        }

        /// <summary>
        /// end processing
        /// </summary>
        protected override void EndProcessing()
        {
            double timespan = OperationContext.GetRunningMilliseconds();
            string message = string.Format(Resources.EndProcessingLog,
                this.GetType().Name, remoteCallCounter, timespan, OperationContext.ClientRequestID);
            WriteDebugLog(message);
            base.EndProcessing();
        }

        /// <summary>
        /// stop processing
        /// time-consuming operation should work with ShouldForceQuit
        /// </summary>
        protected override void StopProcessing()
        {
            //ctrl + c and etc
            ShouldForceQuit = true;
            base.StopProcessing();
        }
    }
}
