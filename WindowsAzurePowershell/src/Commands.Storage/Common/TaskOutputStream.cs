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

namespace Microsoft.WindowsAzure.Commands.Storage.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TaskOutputStream
    {
        /// <summary>
        /// Ouput Stream which store all the output from sub-thread.
        /// Both the object and exception are the valid output.
        /// Key: Output id
        /// Value: OutputUnit object which store the output data
        /// </summary>
        private ConcurrentDictionary<long, OutputUnit> OutputStream;

        /// <summary>
        /// Current output id
        /// </summary>
        private long CurrentOutputId;
        
        /// <summary>
        /// Main thread output writer. WriteObject is a good candidate for it.
        /// </summary>
        public Action<object> OutputWriter;

        /// <summary>
        /// Main thread error writer. WriteError is a goog candidate for it.
        /// </summary>
        public Action<Exception> ErrorWriter;

        public Action<string> VerboseWriter;

        public Action<ProgressRecord> ProgressWriter;

        public Action<string> DebugWriter;

        public Func<string, string, string, bool> ConfirmWriter;

        public Func<long, bool> TaskStatusQueryer;

        /// <summary>
        /// The operation that should be confirmed by user.
        /// </summary>
        private Lazy<ConcurrentQueue<ConfirmTaskCompletionSource>> ConfirmQueue;

        private ConcurrentQueue<string> DebugMessages;

        private ConcurrentQueue<ProgressRecord> Progress;

        /// <summary>
        /// Create an OrderedStreamWriter
        /// </summary>
        public TaskOutputStream()
        {
            OutputStream = new ConcurrentDictionary<long, OutputUnit>();
            CurrentOutputId = 0;
            ConfirmQueue = new Lazy<ConcurrentQueue<ConfirmTaskCompletionSource>>(
                () => new ConcurrentQueue<ConfirmTaskCompletionSource>(), true);
            DebugMessages = new ConcurrentQueue<string>();
            Progress = new ConcurrentQueue<ProgressRecord>();
        }

        /// <summary>
        /// Write output unit into OutputStream
        /// </summary>
        /// <param name="id">Output id</param>
        /// <param name="unit">Output unit</param>
        private void WriteOutputUnit(long id, OutputUnit unit)
        {
            OutputStream.AddOrUpdate(id, unit, (key, oldUnit) => 
                {
                    //Merge data queue
                    List<object> newDataQueue = unit.DataQueue.ToList();
                    foreach (object data in newDataQueue)
                    {
                        oldUnit.DataQueue.Enqueue(data);
                    }
                    return oldUnit;
                });
        }

        /// <summary>
        /// Write object into OutputStream
        /// </summary>
        /// <param name="taskId">Output id</param>
        /// <param name="data">Output data</param>
        public void WriteObject(long taskId, object data)
        {
            OutputUnit unit = new OutputUnit(data, OutputType.Object);
            WriteOutputUnit(taskId, unit);
        }

        /// <summary>
        /// Write error into OutputStream
        /// </summary>
        /// <param name="taskId">Output id</param>
        /// <param name="e">Exception object</param>
        public void WriteError(long taskId, Exception e)
        {
            OutputUnit unit = new OutputUnit(e, OutputType.Error);
            WriteOutputUnit(taskId, unit);
        }

        public void WriteVerbose(long taskId, string message)
        {
            OutputUnit unit = new OutputUnit(message, OutputType.Verbose);
            WriteOutputUnit(taskId, unit);
        }

        public void WriteProgress(ProgressRecord record)
        {
            Progress.Enqueue(record);
        }

        public void WriteDebug(string message)
        {
            DebugMessages.Enqueue(message);
        }

        public bool IsTaskDone(long taskId)
        {
            if (TaskStatusQueryer == null)
            {
                return true;
            }
            else
            {
                return TaskStatusQueryer(taskId);
            }
        }

        /// <summary>
        /// Asyc confirm to continue.
        /// *****Please note*****
        /// Dead lock will happen if the main thread is blocked.
        /// </summary>
        /// <param name="message">Confirm message</param>
        public Task<bool> ConfirmAsyc(string message)
        {
            ConfirmTaskCompletionSource tcs = new ConfirmTaskCompletionSource(message);
            ConfirmQueue.Value.Enqueue(tcs);
            return tcs.Task;
        }

        internal void ConfirmRequest(ConfirmTaskCompletionSource tcs)
        {
            bool result = ConfirmWriter(string.Empty, tcs.Message, Resources.ConfirmCaption);
            tcs.SetResult(result);
        }

        protected void ProcessConfirmRequest()
        {
            if (ConfirmQueue.IsValueCreated)
            {
                ConfirmTaskCompletionSource tcs = null;
                while (ConfirmQueue.Value.TryDequeue(out tcs))
                {
                    ConfirmRequest(tcs);
                }
            }
        }

        protected void ProcessDebugMessages()
        {
            ProcessUnorderedOutputStream<string>(DebugMessages, DebugWriter);
        }

        protected void ProcessProgress()
        {
            ProcessUnorderedOutputStream<ProgressRecord>(Progress, ProgressWriter);
        }

        protected void ProcessUnorderedOutputStream<T>(ConcurrentQueue<T> data, Action<T> Writer)
        {
            int count = data.Count;
            T message = default(T);
            bool removed = false;

            while (count > 0)
            {
                removed = data.TryDequeue(out message);
                count--;

                if (removed)
                {
                    try
                    {
                        Writer(message);
                    }
                    catch (Exception e)
                    {
                        Debug.Fail(e.Message);
                        return;
                    }
                }
            }
        }

        protected void ProcessDataOutput()
        {
            OutputUnit unit = null;
            bool removed = false;
            bool taskDone = false;

            do
            {
                taskDone = IsTaskDone(CurrentOutputId);

                removed = OutputStream.TryRemove(CurrentOutputId, out unit);

                if (removed)
                {
                    try
                    {
                        object data = null;
                        while (unit.DataQueue.TryDequeue(out data))
                        {
                            switch (unit.Type)
                            {
                                case OutputType.Object:
                                    OutputWriter(data);
                                    break;
                                case OutputType.Error:
                                    ErrorWriter(data as Exception);
                                    break;
                                case OutputType.Verbose:
                                    VerboseWriter(data as string);
                                    break;
                            }
                        }
                    }
                    catch (PipelineStoppedException)
                    {
                        //Directly stop the output stream when throw an exception.
                        //If so, we could quickly response for ctrl + c and etc.
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.Fail(String.Format("{0}", e));
                        break;
                    }
                }

                if (taskDone)
                {
                    CurrentOutputId++;
                }   //Otherwise wait for the task completion
            }
            while (taskDone);
        }

        /// <summary>
        /// Output data into main thread
        /// There is no concurrent call on this method since it should be only called in main thread of the powershell instance.
        /// </summary>
        public void Output()
        {
            ProcessConfirmRequest();
            ProcessDebugMessages();
            ProcessProgress();
            ProcessDataOutput();
        }

        /// <summary>
        /// Output type
        /// </summary>
        private enum OutputType
        {
            Object,
            Error,
            Verbose
        };

        /// <summary>
        /// Output unit
        /// </summary>
        private class OutputUnit
        {
            /// <summary>
            /// Output type
            /// </summary>
            public OutputType Type;

            /// <summary>
            /// Output list
            /// All the output unit which has the same output key will be merged in the OutputStream
            /// </summary>
            public ConcurrentQueue<object> DataQueue;

            /// <summary>
            /// Create an OutputUnit
            /// </summary>
            /// <param name="type">Output type</param>
            /// <param name="data">Output data</param>
            public OutputUnit(object data, OutputType type)
            {
                Type = type;
                DataQueue = new ConcurrentQueue<object>();
                DataQueue.Enqueue(data);
            }
        }
    }
}
