// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Storage.Blob
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure.Commands.Storage.Common;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.DataMovement;

    public class StorageDataMovementCmdletBase : StorageCloudBlobCmdletBase, IDisposable
    {
        /// <summary>
        /// Blob Transfer Manager
        /// </summary>
        protected BlobTransferManager transferManager;

        [Parameter(HelpMessage = "Force to overwrite the existing blob or file")]
        public SwitchParameter Force
        {
            get { return overwrite; }
            set { overwrite = value; }
        }

        protected bool overwrite;

        /// <summary>
        /// Confirm the overwrite operation
        /// </summary>
        /// <param name="msg">Confirmation message</param>
        /// <returns>True if the opeation is confirmed, otherwise return false</returns>
        internal virtual bool ConfirmOverwrite(string sourcePath, string destinationPath)
        {
            string overwriteMessage = String.Format(Resources.OverwriteConfirmation, destinationPath);
            return overwrite || OutputStream.ConfirmAsync(overwriteMessage).Result;
        }

        /// <summary>
        /// On Task run successfully
        /// </summary>
        /// <param name="data">User data</param>
        protected virtual void OnTaskSuccessful(DataMovementUserData data)
        { }

        /// <summary>
        /// on data movement job start
        /// </summary>
        /// <param name="data">User data</param>
        protected virtual void OnDMJobStart(object data)
        {
            if (ShouldForceQuit)
            {
                return;
            }

            DataMovementUserData userData = data as DataMovementUserData;
            if (null == userData)
            {
                return;
            }

            ProgressRecord pr = userData.Record;

            if (null != pr)
            {
                pr.PercentComplete = 0;
                OutputStream.WriteProgress(pr);
            }
        }

        /// <summary>
        /// on data movement job progress 
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="speed">download speed</param>
        /// <param name="percent">download percent</param>
        protected virtual void OnDMJobProgress(object data, double speed, double percent)
        {
            if (ShouldForceQuit)
            {
                return;
            }

            DataMovementUserData userData = data as DataMovementUserData;

            if (null == userData)
            {
                return;
            }

            ProgressRecord pr = userData.Record;

            pr.PercentComplete = (int)percent;
            pr.StatusDescription = String.Format(Resources.FileTransmitStatus, pr.PercentComplete, Util.BytesToHumanReadableSize(speed));
            OutputStream.WriteProgress(pr);
        }

        /// <summary>
        /// on data movement job finish
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="e">run time exception</param>
        protected virtual void OnDMJobFinish(object data, Exception e)
        {
            try
            {
                if (ShouldForceQuit)
                {
                    return;
                }

                DataMovementUserData userData = data as DataMovementUserData;

                string status = string.Empty;

                if (null == e)
                {
                    try
                    {
                        OnTaskSuccessful(userData);
                    }
                    catch (Exception postProcessException)
                    {
                        e = postProcessException;
                    }
                }

                if (null == e)
                {
                    status = Resources.TransmitSuccessfully;
                    userData.taskSource.SetResult(true);
                }
                else
                {
                    status = String.Format(Resources.TransmitFailed, e.Message);
                    userData.taskSource.SetException(e);
                }

                if (userData != null && userData.Record != null)
                {
                    if (e == null)
                    {
                        userData.Record.PercentComplete = 100;
                    }

                    userData.Record.StatusDescription = status;
                    OutputStream.WriteProgress(userData.Record);
                }
            }
            catch (Exception unknownException)
            {
                Debug.Fail(unknownException.ToString());
            }
        }

        /// <summary>
        /// Cmdlet begin processing
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            BlobTransferOptions opts = new BlobTransferOptions();
            opts.Concurrency = GetCmdletConcurrency();
            opts.AppendToClientRequestId(CmdletOperationContext.ClientRequestId);
            opts.OverwritePromptCallback = ConfirmOverwrite;
            SetRequestOptionsInDataMovement(opts);
            transferManager = new BlobTransferManager(opts);
            CmdletCancellationToken.Register(() => transferManager.CancelWork());
        }

        private void SetRequestOptionsInDataMovement(BlobTransferOptions opts)
        {
            BlobRequestOptions cmdletOptions = RequestOptions;

            if (cmdletOptions == null)
            {
                return;
            }
            
            foreach (BlobRequestOperation operation in Enum.GetValues(typeof(BlobRequestOperation)))
            {
                BlobRequestOptions requestOptions = opts.GetBlobRequestOptions(operation);

                if (cmdletOptions.MaximumExecutionTime != null)
                {
                    requestOptions.MaximumExecutionTime = cmdletOptions.MaximumExecutionTime;
                }

                if (cmdletOptions.ServerTimeout != null)
                {
                    requestOptions.ServerTimeout = cmdletOptions.ServerTimeout;
                }

                opts.SetBlobRequestOptions(operation, requestOptions);    
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteTaskSummary();
        }

        /// <summary>
        /// Dispose DataMovement cmdlet
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose DataMovement cmdlet
        /// </summary>
        /// <param name="disposing">User disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (transferManager != null)
                {
                    transferManager.WaitForCompletion();
                    transferManager.Dispose();
                    transferManager = null;
                }
            }
        }
    }
}
