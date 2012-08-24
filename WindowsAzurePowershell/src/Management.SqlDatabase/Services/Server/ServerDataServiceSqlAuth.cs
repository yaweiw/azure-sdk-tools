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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Net;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;

    /// <summary>
    /// Implementation of the ServerDataService with Sql Authentication.
    /// </summary>
    public partial class ServerDataServiceSqlAuth : ServerDataServiceContext
    {
        #region Constants

        /// <summary>
        /// Model name used in the connection type string
        /// </summary>
        private const string ServerModelConnectionType = "Server2";

        /// <summary>
        /// The default dataservicecontext request timeout.
        /// </summary>
        private const int DefaultDataServiceContextTimeoutInSeconds = 180;

        #endregion

        #region Private data

        /// <summary>
        /// An ID that identifies this session for end-to-end tracing
        /// </summary>
        private readonly Guid sessionActivityId;

        /// <summary>
        /// The connection type identifying the model and connection parameters to use
        /// </summary>
        private readonly DataServiceConnectionType connectionType;

        /// <summary>
        /// The access token to use in requests
        /// </summary>
        private readonly AccessTokenResult accessToken;

        /// <summary>
        /// A collection of entries to be added to each request's header. HTTP headers are case-insensitive. 
        /// </summary>
        private readonly Dictionary<string, string> supplementalHeaderEntries =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Serializes some instance-level operations
        /// </summary>
        private readonly object instanceSyncObject = new object();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServiceSqlAuth"/> class.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service Uri.</param>
        /// <param name="connectionType">The server connection type with the server name</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessToken">The authentication access token to be used for executing web requests.</param>
        private ServerDataServiceSqlAuth(
            Uri managementServiceUri,
            DataServiceConnectionType connectionType,
            Guid sessionActivityId,
            AccessTokenResult accessToken)
            : base(new Uri(managementServiceUri, connectionType.RelativeEntityUri))
        {
            this.sessionActivityId = sessionActivityId;
            this.connectionType = connectionType;
            this.accessToken = accessToken;
        }

        #region Public Properties

        /// <summary>
        /// Gets the session activity Id associated with this context.
        /// </summary>
        public Guid SessionActivityId
        {
            get
            {
                return this.sessionActivityId;
            }
        }

        #endregion

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials. If the server name
        /// is null, the default server name from the serviceRoot Uri will be used.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="credentials">The credentials to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri, 
            Guid sessionActivityId, 
            SqlAuthenticationCredentials credentials,
            string serverName)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            // Retrieve GetAccessToken operation Uri
            Uri accessUri = DataConnectionUtility.GetAccessTokenUri(managementServiceUri);

            // Synchronously call GetAccessToken
            AccessTokenResult result = DataServiceAccess.GetAccessToken(accessUri, credentials);

            // Validate the retrieved access token
            AccessTokenResult.ValidateAccessToken(managementServiceUri, result);

            // Create and return a ServerDataService object
            return Create(managementServiceUri, sessionActivityId, result, serverName);
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessTokenResult">The accessToken to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri, 
            Guid sessionActivityId,
            AccessTokenResult accessTokenResult,
            string serverName)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            // Create a ServerDataServiceSqlAuth object
            if (serverName == null)
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType),
                    sessionActivityId,
                    accessTokenResult);
            }
            else
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType, serverName),
                    sessionActivityId,
                    accessTokenResult);
            }
        }

        #region Server Data Service Operations

        /// <summary>
        /// Retrieves the metadata for the context as a <see cref="XDocument"/>
        /// </summary>
        /// <returns>The metadata for the context as a <see cref="XDocument"/></returns>
        public override XDocument RetrieveMetadata()
        {
            XDocument doc = DataConnectionUtility.GetMetadata(this, EnhanceRequest);
            return doc;
        }

        #endregion

        /// <summary>
        /// Sets a supplemental property value that will be send with each request. 
        /// </summary>
        /// <param name="key">A key that uniquely identifies the property</param>
        /// <param name="value">A string representation of the property value</param>
        public void SetSessionProperty(string key, string value)
        {
            lock (this.instanceSyncObject)
            {
                this.supplementalHeaderEntries[key] = value;
            }
        }

        /// <summary>
        /// Handler to add aditional headers and properties to the request.
        /// </summary>
        /// <param name="request">The request to enhance.</param>
        protected override void OnEnhanceRequest(HttpWebRequest request)
        {
            EnhanceRequest(this, request);
        }

        /// <summary>
        /// Enhance a request with auth token.
        /// </summary>
        /// <param name="context">The data service context for the request.</param>
        /// <param name="request">The request.</param>
        private static void EnhanceRequest(ServerDataServiceSqlAuth context, HttpWebRequest request)
        {
            lock (context.instanceSyncObject)
            {
                foreach (KeyValuePair<string, string> entry in context.supplementalHeaderEntries)
                {
                    request.Headers[entry.Key] = entry.Value;
                }
            }

            // Add the UserAgent string
            request.UserAgent = Constants.WindowsAzurePowerShellUserAgent;

            // Add the access token header
            request.Headers[DataServiceConstants.AccessTokenHeader] = context.accessToken.AccessToken;

            // Add the access token cookie
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(context.accessToken.AccessCookie);

            // Add the session activity Id
            request.Headers[DataServiceConstants.SessionTraceActivityHeader] = context.sessionActivityId.ToString();
        }
    }
}
