/**
* Copyright Microsoft Corporation 2012
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

using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Options for how to initialize the ServiceManagementClient.
    /// </summary>
    public class ServiceManagementClientOptions
    {
        /// <summary>
        /// Gets the .Net TraceSource associated with the options.
        /// </summary>
        public TraceSource Logger { get; private set; }

        /// <summary>
        /// Gets the error event ID associated with the options.
        /// </summary>
        public int ErrorEventId { get; private set; }

        /// <summary>
        /// Gets an optional UserAgent string associated with the options.
        /// </summary>
        public string UserAgentString { get; private set; }

        /// <summary>
        /// Gets an optional function to generate client request ID headers associated with the options.
        /// </summary>
        public Func<string> ClientRequestIdGenerator { get; private set; }

        /// <summary>
        /// Gets an optional EndpointBehaviors list of custom client message inspectors.
        /// </summary>
        public IList<IEndpointBehavior> EndpointBehaviors { get; private set; }

        /// <summary>
        /// Gets a default set of Service Management Client options.
        /// </summary>
        public static readonly ServiceManagementClientOptions DefaultOptions = new ServiceManagementClientOptions();

        /// <summary>
        /// Creates a new instance of ServiceManagementClientOptions.
        /// </summary>
        public ServiceManagementClientOptions()
            : this(null, null, null, 0)
        {
            // Empty
        }

        /// <summary>
        /// Creates a new instance of ServiceManagementClientOptions with a .Net TraceSource for logging.
        /// </summary>
        /// <param name="logger">.Net TraceSource to use for logging.</param>
        public ServiceManagementClientOptions(TraceSource logger)
            : this(null, null, logger, 0)
        {
            // Empty
        }

        /// <summary>
        /// Creates a new instance of ServiceManagementClientOptions with .Net TraceSource for logging.
        /// </summary>
        /// <param name="logger">.Net TraceSource to use for logging.</param>
        /// <param name="errorEventId">ID for the event to use when logging errors.</param>
        /// <summary>
        public ServiceManagementClientOptions(TraceSource logger, int errorEventId)
            : this(null, null, logger, errorEventId)
        {
            // Empty
        }

        /// <summary>
        /// Creates a new instance of ServiceManagementClientOptions with .Net TraceSource for logging.
        /// </summary>
        /// <param name="userAgent">User agent string to supply with requests.</param>
        /// <param name="clientRequestIdGenerator">Function delegate for returning a x-ms-clientid value.</param>
        /// <param name="logger">.Net TraceSource to use for logging.</param>
        /// <param name="errorEventId">ID for the event to use when logging errors.</param>
        /// <summary>
        public ServiceManagementClientOptions(string userAgent, Func<string> clientRequestIdGenerator, TraceSource logger, int errorEventId)
        {
            this.UserAgentString = userAgent;
            this.ClientRequestIdGenerator = clientRequestIdGenerator;
            this.Logger = logger;
            this.ErrorEventId = errorEventId;
            this.EndpointBehaviors = new List<IEndpointBehavior>();
        }

        /// <summary>
        /// Creates a new instance of ServiceManagementClientOptions with .Net TraceSource for logging.
        /// </summary>
        /// <param name="messageInspectors">List of custom endpoint behaviors.</param>
        /// <summary>
        public ServiceManagementClientOptions(IList<IEndpointBehavior> messageInspectors)
        {
            this.EndpointBehaviors = messageInspectors;
        }

    }
}
