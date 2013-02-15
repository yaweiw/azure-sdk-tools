/**
* Copyright Microsoft Corporation  2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Helper class for tracing.
    /// </summary>
    internal class TraceSourceHelper
    {
        private TraceSource _debugTrace = new TraceSource("ServiceManagementInternalDebug", SourceLevels.Verbose);
        private TraceSource _logger;
        private int _errorEventId = 0;
        private string _componentNameString;

        /// <summary>
        /// Constructs a new instance of the tracing helper.
        /// </summary>
        /// <param name="logger">The optional .Net TraceSource to be used for tracing.</param>
        /// <param name="errorEventId">The optional event ID to be associated with errors.</param>
        /// <param name="componentNameString">The name of the component doing the tracing.</param>
        public TraceSourceHelper(TraceSource logger, int errorEventId, string componentNameString)
        {
            this._logger = logger;
            this._errorEventId = errorEventId;
            this._componentNameString = componentNameString;
        }

        /// <summary>
        /// Traces an information event.
        /// </summary>
        /// <param name="msg">The message associated with the trace.</param>
        public void LogInformation(string msg)
        {
            this.TraceInternal(TraceEventType.Information, msg);

        }

        /// <summary>
        /// Traces an information event.
        /// </summary>
        /// <param name="format">The format string associated with the trace.</param>
        /// <param name="args">The format string arguments associated with the trace.</param>
        public void LogInformation(string format, params object[] args)
        {
            this.TraceInternal(TraceEventType.Information, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <summary>
        /// Traces an error event.
        /// </summary>
        /// <param name="msg">The message associated with the trace.</param>
        public void LogError(string msg)
        {
            this.TraceInternal(TraceEventType.Error, msg);
        }

        /// <summary>
        /// Traces an error event.
        /// </summary>
        /// <param name="format">The format string associated with the trace.</param>
        /// <param name="args">The format string arguments associated with the trace.</param>
        public void LogError(string format, params object[] args)
        {
            this.TraceInternal(TraceEventType.Error, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <summary>
        /// Traces a debug event.
        /// </summary>
        /// <param name="msg">The message associated with the trace.</param>
        public void LogDebugInformation(string format, params object[] args)
        {
            if (this._debugTrace != null)
            {
                this._debugTrace.TraceInformation(this.GenerateLogStamp() + format, args);
            }
        }

        /// <summary>
        /// Traces a debug event.
        /// </summary>
        /// <param name="format">The format string associated with the trace.</param>
        /// <param name="args">The format string arguments associated with the trace.</param>
        public void LogDebugInformation(string msg)
        {
            if (this._debugTrace != null)
            {
                this._debugTrace.TraceInformation(this.GenerateLogStamp() + msg);
            }
        }

        #region Private Methods

        private void TraceInternal(TraceEventType eventKind, string msg)
        {
            if (this._logger != null)
            {
                switch (eventKind)
                {
                    case TraceEventType.Information:
                        this._logger.TraceInformation(string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.GenerateLogStamp(), msg));
                        break;
                    case TraceEventType.Error:
                        this._logger.TraceEvent(TraceEventType.Error, this._errorEventId, string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.GenerateLogStamp(), msg));                        
                        break;
                    default:
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.UnrecognizedTraceType, eventKind), "eventKind");
                }            
            }

            // Always log for Microsoft debugging purposes
            this.LogDebugInformation(msg);
        }

        private string GenerateLogStamp()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} - {1}:", DateTimeOffset.UtcNow.ToString(), this._componentNameString);
        }

        #endregion
    }
}
