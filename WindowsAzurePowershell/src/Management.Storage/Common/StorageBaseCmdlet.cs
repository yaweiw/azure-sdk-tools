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

    public class StorageBaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        [Parameter(HelpMessage = "Azure Storage Context Object",
            ValueFromPipelineByPropertyName = true)]
        public StorageContext Context {get; set;}

        protected OperationContext operationContext = null;
        private int restCallCount = 0;

        internal void InitOperationContext()
        {
            operationContext = new OperationContext();
            operationContext.StartTime = DateTime.Now;
            operationContext.SendingRequest += (s, e) =>
                {
                    restCallCount++;
                    string message = String.Format("Start {0}th remote call: {1} {2}",
                        restCallCount, e.Request.Method, e.Request.RequestUri.ToString());
                    SafeWriteVerboseLog(message);
                };
            //FIXME can not work with ctrl + c
            operationContext.ResponseReceived += (s, e) =>
                {
                    string message = String.Format("Finish remote call with status code {0} and service request id {1}",
                        e.Response.StatusCode, e.RequestInformation.ServiceRequestID);
                    SafeWriteVerboseLog(message);
                };
            operationContext.ClientRequestID = GetClientRequestID();
            SafeWriteVerboseLog("Init Operation Context with operation id " + operationContext.ClientRequestID);
        }

        internal string GetClientRequestID()
        {
            string prefix = "Azure-Storage-PowerShell-";
            string uniqueId = System.Guid.NewGuid().ToString();
            return prefix + uniqueId;
        }

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
                StorageContext context = new StorageContext();
                context.StorageAccount = account;
                Context = context;
                return account;
            }
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
                throw new ArgumentException(Resource.StorageCredentialsNotFound);
            }
            else
            {
                StorageCredentials credential = new StorageCredentials(accountName, accountKey);
                return new CloudStorageAccount(credential, true);
            }
        }

        protected CloudBlobClient GetCloudBlobClient()
        {
            //use the default retry policy in storage client
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudBlobClient();
        }

        protected CloudQueueClient GetCloudQueueClient()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudQueueClient();
        }

        protected CloudTableClient GetCloudTableClient()
        {
            CloudStorageAccount account = GetCloudStorageAccount();
            return account.CreateCloudTableClient();
        }

        protected virtual void WriteErrorDetails(StorageException exception)
        {
            ErrorCategory errorCategory = ErrorCategory.CloseError;
            exception = StorageExceptionUtil.RepackStorageException(exception);
            SafeWriteError(new ErrorRecord(exception, exception.GetType().Name, errorCategory, null));
        }

        protected override void SafeWriteError(Exception e)
        {
            Debug.Assert(e != null, "ex cannot be null or empty.");

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
            SafeWriteError(new ErrorRecord(e, e.GetType().Name, errorCategory, null));
        }

        internal void SafeWriteObjectWithContext(AzureStorageBase item)
        {
            item.Context = Context;
            WriteOutputObject(item);
        }

        //FIXME tips should not in pipeline and can not be sorted.
        internal void SafeWriteTips(string message)
        {
            WriteOutputObject(message);
        }

        internal void SafeWriteVerboseLog(string msg)
        {
            string time = DateTime.Now.ToString();
            string log = String.Format("{0} {1}", time, msg);
            SafeWriteVerbose(log);
        }

        internal double GetRunningMilliseconds()
        {
            if(operationContext == null)
            {
                return 0;
            }
            TimeSpan span = DateTime.Now - operationContext.StartTime;
            return span.TotalMilliseconds;
        }

        protected override void BeginProcessing()
        {
            InitOperationContext();
            SafeWriteVerboseLog(this.GetType().Name + " begin processing");
            base.BeginProcessing();
        }

        internal virtual void ExecuteCommand() 
        {
            return;
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = Context != null;

                base.ProcessRecord();

                this.ExecuteCommand();
            }
            catch (Exception e)
            {
                SafeWriteError(e);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            double timespan = GetRunningMilliseconds();
            string message = string.Format("{0} end processing, Use {1} remote calls. Elapsed time {2:0.00} ms. Operation id: {3}",
                this.GetType().Name, restCallCount, timespan, operationContext.ClientRequestID);
            SafeWriteVerboseLog(message);
        }

        //FIXME can not be called
        protected override void StopProcessing()
        {
            double timespan = GetRunningMilliseconds();
            string message = string.Format("{0} stop processing, Use {1} remote calls. Elapsed time {2:0.00} ms. Operation id: {3}",
                this.GetType().Name, restCallCount, timespan, operationContext.ClientRequestID);
            SafeWriteVerboseLog(message);
            base.StopProcessing();
        }
    }
}
