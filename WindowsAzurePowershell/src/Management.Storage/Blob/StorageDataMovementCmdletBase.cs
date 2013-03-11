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

namespace Microsoft.WindowsAzure.Management.Storage.Blob
{
    using Microsoft.WindowsAzure.Storage.DataMovement;
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Threading;

    public class StorageDataMovementCmdletBase : StorageCloudBlobCmdletBase, IDisposable
    {
        /// <summary>
        /// Amount of concurrent async tasks to run per available core.
        /// </summary>
        protected int concurrentTaskCount = 0;

        /// <summary>
        /// whether the transfer progress finished
        /// </summary>
        private bool finished;

        /// <summary>
        /// exception thrown during transfer
        /// </summary>
        private Exception runtimeException;

        /// <summary>
        /// Blob Transfer Manager
        /// </summary>
        private BlobTransferManager transferManager;

        /// <summary>
        /// Default task per core
        /// </summary>
        private const int asyncTasksPerCoreMultiplier = 8;

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
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual bool ConfirmOverwrite(string destinationPath)
        {
            string overwriteMessage = String.Format(Resources.OverwriteConfirmation, destinationPath);
            return overwrite || ShouldProcess(destinationPath);
        }

        /// <summary>
        /// on download start
        /// </summary>
        /// <param name="progress">progress information</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnTaskStart(object progress)
        {
            ProgressRecord pr = progress as ProgressRecord;

            if (null != pr)
            {
                pr.PercentComplete = 0;
            }
        }

        /// <summary>
        /// on downloading 
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="speed">download speed</param>
        /// <param name="percent">download percent</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnTaskProgress(object progress, double speed, double percent)
        {
            ProgressRecord pr = progress as ProgressRecord;

            if (null == pr)
            {
                return;
            }

            pr.PercentComplete = (int)percent;
            pr.StatusDescription = String.Format(Resources.FileTransmitStatus, pr.PercentComplete, speed);
        }

        /// <summary>
        /// on downloading finish
        /// </summary>
        /// <param name="progress">progress information</param>
        /// <param name="e">run time exception</param>
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        internal virtual void OnTaskFinish(object progress, Exception e)
        {
            finished = true;
            runtimeException = e;

            ProgressRecord pr = progress as ProgressRecord;

            if (null == pr)
            {
                return;
            }

            pr.PercentComplete = 100;

            if (null == e)
            {
                pr.StatusDescription = Resources.TransmitSuccessfully;
            }
            else
            {
                pr.StatusDescription = String.Format(Resources.TransmitFailed, e.Message);
            }
        }

        protected override void BeginProcessing()
        {
            if (concurrentTaskCount == 0)
            {
                concurrentTaskCount = Environment.ProcessorCount * asyncTasksPerCoreMultiplier;
            }

            BlobTransferOptions opts = new BlobTransferOptions();
            opts.OverwritePromptCallback = ConfirmOverwrite;
            opts.Concurrency = concurrentTaskCount;
            transferManager = new BlobTransferManager(opts);
            
            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            if (transferManager != null)
            {
                transferManager.WaitForCompletion();
                transferManager.Dispose();
                transferManager = null;
            }

            base.EndProcessing();
        }

        protected void StartSyncTaskInTransferManager(Action<BlobTransferManager> taskAction, ProgressRecord record = null)
        {
            finished = false;

            //status update interval
            int interval = 1 * 1000; //in millisecond

            taskAction(transferManager);

            while (!finished)
            {
                if (record != null)
                {
                    WriteProgress(record);
                }

                Thread.Sleep(interval);

                if (ShouldForceQuit)
                {
                    //can't output verbose log for this operation since the Output stream is already stopped.
                    transferManager.CancelWork();
                    break;
                }
            }

            if (runtimeException != null)
            {
                throw runtimeException;
            }
        }

        public void Dispose()
        {
            if (transferManager != null)
            {
                transferManager.Dispose();
                transferManager = null;
            }
        }
    }
}
