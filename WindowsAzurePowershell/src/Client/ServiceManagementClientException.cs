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

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Represents exceptions on the wire when calling the Service Management API.
    /// </summary>
    [Serializable]
    public class ServiceManagementClientException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code of the failed Service Management request.
        /// </summary>
        public HttpStatusCode HttpStatus { get; private set; }

        /// <summary>
        /// Gets the error details of the failed Service Management request.
        /// </summary>
        public ServiceManagementError ErrorDetails { get; private set; }

        /// <summary>
        /// Gets the operation tracking ID if called asynchronously of the failed Service Management request.
        /// </summary>
        public string OperationTrackingId { get; private set; }

        /// <summary>
        /// Gets the headers associated with the response from the request that caused the exception.
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; private set; }

        /// <summary>
        /// Constructs a new instance of ServiceManagementClientException.
        /// </summary>
        /// <param name="httpStatus">The HTTP status code of the failed Service Management request.</param>
        /// <param name="errorDetails">The error details of the failed Service Management request.</param>
        /// <param name="operationTrackingId">The operation tracking ID if called asynchronously of the failed Service Management request.</param>
        public ServiceManagementClientException(HttpStatusCode httpStatus, ServiceManagementError errorDetails, string operationTrackingId)
            : this(httpStatus, errorDetails, operationTrackingId, null)
        {
            // Empty
        }

        /// <summary>
        /// Constructs a new instance of ServiceManagementClientException.
        /// </summary>
        /// <param name="httpStatus">The HTTP status code of the failed Service Management request.</param>
        /// <param name="errorDetails">The error details of the failed Service Management request.</param>
        /// <param name="operationTrackingId">The operation tracking ID if called asynchronously of the failed Service Management request.</param>
        /// <param name="responseHeaders">Optional WebResponse containing the original response object from the server</param>
        public ServiceManagementClientException(HttpStatusCode httpStatus, ServiceManagementError errorDetails, string operationTrackingId, WebHeaderCollection responseHeaders)
            : base(string.Format(CultureInfo.CurrentCulture,
                Resources.ServiceManagementClientExceptionStringFormat,
                (int)httpStatus,
                (errorDetails != null) && !string.IsNullOrEmpty(errorDetails.Code) ? errorDetails.Code : Resources.None,
                (errorDetails != null) && !string.IsNullOrEmpty(errorDetails.Message) ? errorDetails.Message : Resources.None,
                string.IsNullOrEmpty(operationTrackingId) ? Resources.None : operationTrackingId))
        {
            this.HttpStatus = httpStatus;
            this.ErrorDetails = errorDetails;
            this.OperationTrackingId = operationTrackingId;
            this.ResponseHeaders = responseHeaders;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
