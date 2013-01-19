// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Sync;
using Microsoft.WindowsAzure.Sync.Upload;
using Microsoft.WindowsAzure.Tools.Vhd.Model;
using ProgressRecord = Microsoft.WindowsAzure.Sync.ProgressRecord;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
    using System;
    using System.IO;

    public class VhdUploadContext
    {
        public FileInfo LocalFilePath { get; set; }
        public Uri DestinationUri { get; set; }
    }

    public class PSSyncOutputEvents : ISyncOutputEvents, IDisposable
    {
        private readonly PSCmdlet cmdlet;
        private Runspace runspace;
        private bool disposed;

        public PSSyncOutputEvents(PSCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
            this.runspace = RunspaceFactory.CreateRunspace(this.cmdlet.Host);
            this.runspace.Open();
        }

        private static string FormatDuration(TimeSpan ts)
        {
            if (ts.Days == 0)
            {
                return String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }
            return String.Format("{0} days {1:00}:{2:00}:{3:00}", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        }

        public void ProgressCopyStatus(ProgressRecord record)
        {
            ProgressCopyStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressCopyStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress(0, "Copying", precentComplete, remainingTime, avgThroughputMbps);
        }

        public void ProgressCopyComplete(TimeSpan elapsed)
        {
            LogProgressComplete(0, "Copying");
            LogMessage("Elapsed time for copy: {0}", FormatDuration(elapsed));
        }

        public void ProgressUploadStatus(ProgressRecord record)
        {
            ProgressUploadStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressUploadStatus(double precentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress(0, "Uploading", precentComplete, remainingTime, avgThroughputMbps);
        }

        private void LogProgress(int activityId, string activity, double precentComplete, TimeSpan remainingTime, double avgThroughputMbps)
        {
            var message = String.Format("{0:0.0}% complete; Remaining Time: {1}; Throughput: {2:0.0}Mbps",
                                        precentComplete,
                                        FormatDuration(remainingTime),
                                        avgThroughputMbps);
            var progressCommand = String.Format(@"Write-Progress -Id {0} -Activity '{1}' -Status '{2}' -SecondsRemaining {3} -PercentComplete {4}", activityId, activity, message, (int) remainingTime.TotalSeconds, (int) precentComplete);
            using(var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddScript(progressCommand);
                ps.Invoke();
            }
        }

        private void LogProgressComplete(int activityId, string activity)
        {
            var progressCommand = String.Format(@"Write-Progress -Id {0} -Activity '{1}' -Status '{2}' -Completed", activityId, activity, "Completed");
            using(var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddScript(progressCommand);
                var result = ps.Invoke();
            }
        }

        public void MessageCreatingNewPageBlob(long pageBlobSize)
        {
            LogMessage("Creating new page blob of size {0}...", pageBlobSize);
        }

        private void LogMessage(string format, params object[] parameters)
        {
            var message = String.Format(format, parameters);
            var verboseMessage = String.Format("Write-Host '{0}'", message);
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddScript(verboseMessage);
                ps.Invoke();
            }
        }

        private void LogError(Exception e)
        {
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddCommand("Write-Error");
                ps.AddParameter("ErrorRecord", new ErrorRecord(e, String.Empty, ErrorCategory.NotSpecified, null));
                ps.Invoke();
            }
        }

        public void MessageResumingUpload()
        {
            LogMessage("Found existing page blob. Resuming upload...");
        }

        public void ProgressUploadComplete(TimeSpan elapsed)
        {
            LogProgressComplete(0, "Uploading");
            LogMessage("Elapsed time for upload: {0}", FormatDuration(elapsed));
        }

        public void ProgressOperationStatus(ProgressRecord record)
        {
            ProgressOperationStatus(record.PercentComplete, record.AvgThroughputMbPerSecond, record.RemainingTime);
        }

        public void ProgressOperationStatus(double percentComplete, double avgThroughputMbps, TimeSpan remainingTime)
        {
            LogProgress(1, "Calculating MD5 Hash", percentComplete, remainingTime, avgThroughputMbps);
        }

        public void ProgressOperationComplete(TimeSpan elapsed)
        {
            LogProgressComplete(1, "Calculating MD5 Hash");
            LogMessage("Elapsed time for the operation: {0}", FormatDuration(elapsed));
        }


        public void ErrorUploadFailedWithExceptions(IList<Exception> exceptions)
        {
            LogMessage("Upload failed with exceptions:");
            foreach (var exception in exceptions)
            {
                LogError(exception);
            }
        }

        public void MessageCalculatingMD5Hash(string filePath)
        {
            LogMessage("MD5 hash is being calculated for the file '{0}'.", filePath);
        }

        public void MessageMD5HashCalculationFinished()
        {
            LogMessage("MD5 hash calculation is completed.");
        }

        public void MessageRetryingAfterANetworkDisruption()
        {
            LogMessage("Network disruption occured, retrying.");
        }

        public void DebugRetryingAfterException(Exception lastException)
        {
            LogDebug(lastException.ToString());

            var storageException = lastException as StorageException;
            var message = ExceptionUtil.DumpStorageExceptionErrorDetails(storageException);
            if (message != String.Empty)
            {
                LogDebug(message);
            }
        }

        public void MessageDetectingActualDataBlocks()
        {
            LogMessage("Detecting the empty data blocks in the local file.");
        }

        public void MessageDetectingActualDataBlocksCompleted()
        {
            LogMessage("Detecting the empty data blocks completed.");
        }

        public void MessagePrintBlockRange(IndexRange range)
        {
            LogMessage("Range of the block is {0}, Length: {1}", range, range.Length);
        }

        public void DebugEmptyBlockDetected(IndexRange range)
        {
            LogDebug("Empty block detected: {0}", range.ToString());
        }

        private void LogDebug(string format, params object[] parameters)
        {
            var message = String.Format(format, parameters);
            var debugMessage = String.Format("Write-Debug -Message '{0}'", message);
            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.Runspace = runspace;
                ps.AddScript(debugMessage);
                ps.Invoke();
            }
        }

        public void ProgressEmptyBlockDetection(int processedRangeCount, int totalRangeCount)
        {
            using(var ps = System.Management.Automation.PowerShell.Create())
            {
                if (processedRangeCount >= totalRangeCount)
                {

                    var progressCommand1 = String.Format(@"Write-Progress -Id {0} -Activity '{1}' -Status '{2}' -Completed", 2, "Empty Block Detection", "Completed");
                    ps.Runspace = runspace;
                    ps.AddScript(progressCommand1);
                    ps.Invoke();
                    return;
                }

                var progressCommand = String.Format(@"Write-Progress -Id {0} -Activity '{1}' -Status '{2}' -SecondsRemaining {3} -PercentComplete {4}", 2, "Empty Block Detection", "Detecting empty blocks", -1, ((double)processedRangeCount / totalRangeCount) * 100);
                ps.Runspace = runspace;
                ps.AddScript(progressCommand);
                ps.Invoke();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if (disposing)
                {
                    runspace.Dispose();
                }
                this.disposed = true;
            }
        }
    }
}