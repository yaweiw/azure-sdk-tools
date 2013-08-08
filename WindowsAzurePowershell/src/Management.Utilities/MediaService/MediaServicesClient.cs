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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.WindowsAzure.Management.Utilities.Common;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;
using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
using Microsoft.WindowsAzure.ServiceManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.Management.Utilities.MediaService
{
    /// <summary>
    ///     Implements IMediaServicesClient to use HttpClient for communication
    /// </summary>
    public class MediaServicesClient : IMediaServicesClient
    {
        public const string MediaServiceVersion = "2013-03-01";
        private readonly HttpClient _httpClient;
        private readonly string _subscriptionId;

        /// <summary>
        ///     Creates new MediaServicesClient.
        /// </summary>
        /// <param name="subscription">The Windows Azure subscription data object</param>
        /// <param name="logger">The logger action</param>
        public MediaServicesClient(SubscriptionData subscription, Action<string> logger)
        {
            _subscriptionId = subscription.SubscriptionId;
            Subscription = subscription;
            Logger = logger;
            _httpClient = CreateIMediaServicesHttpClient();
        }

        /// <summary>
        ///     Gets or sets the subscription.
        /// </summary>
        /// <value>
        ///     The subscription.
        /// </value>
        public SubscriptionData Subscription { get; set; }

        /// <summary>
        ///     Gets or sets the logger
        /// </summary>
        /// <value>
        ///     The logger.
        /// </value>
        public Action<string> Logger { get; set; }


        /// <summary>
        ///     Gets the media service accounts async.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<MediaServiceAccount>> GetMediaServiceAccountsAsync()
        {
            return _httpClient.GetAsync(MediaServicesUriElements.Accounts, Logger).ContinueWith(tr => ProcessResponse<IEnumerable<MediaServiceAccount>>(tr));
        }

        /// <summary>
        ///     Gets the media service account details async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Task<MediaServiceAccountDetails> GetMediaServiceAsync(string name)
        {
            return _httpClient.GetAsync(String.Format("{0}/{1}", MediaServicesUriElements.Accounts, name), Logger).ContinueWith(tr => ProcessResponse<MediaServiceAccountDetails>(tr));
        }

        /// <summary>
        ///     Create new azure media service async.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public Task<AccountCreationResult> CreateNewAzureMediaServiceAsync(AccountCreationRequest request)
        {
            return _httpClient.PostAsJsonAsyncWithoutEnsureSuccessStatusCode(MediaServicesUriElements.Accounts, JObject.FromObject(request), Logger).ContinueWith(tr => ProcessResponse<AccountCreationResult>(tr));
        }

        /// <summary>
        ///     Processes the response and handle error cases.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        /// <exception cref="Microsoft.WindowsAzure.ServiceManagement.ServiceManagementClientException"></exception>
        /// <exception cref="ServiceManagementError"></exception>
        private static T ProcessResponse<T>(Task<HttpResponseMessage> responseMessage)
        {
            HttpResponseMessage message = responseMessage.Result;
            string content = message.Content.ReadAsStringAsync().Result;
            if (message.IsSuccessStatusCode)
            {
                return (T) JsonConvert.DeserializeObject(content, typeof (T));
            }
            else
            {
                var doc = new XmlDocument();
                doc.LoadXml(content);
                content = doc.InnerText;
                var serviceError = JsonConvert.DeserializeObject(content, typeof (ServiceError)) as ServiceError;
                throw new ServiceManagementClientException(message.StatusCode,
                                                           new ServiceManagementError
                                                               {
                                                                   Code = message.StatusCode.ToString(),
                                                                   Message = serviceError.Message
                                                               },
                                                           string.Empty);
            }
        }

        /// <summary>
        ///     Creates and initialise instance of HttpClient
        /// </summary>
        /// <returns></returns>
        private HttpClient CreateIMediaServicesHttpClient()
        {
            var requestHandler = new WebRequestHandler();
            requestHandler.ClientCertificates.Add(Subscription.Certificate);
            var endpoint = new StringBuilder(General.EnsureTrailingSlash(Subscription.ServiceEndpoint));
            endpoint.Append(_subscriptionId);

            //Please note that / is nessesary here
            endpoint.Append("/services/mediaservices/");
            HttpClient client = HttpClientHelper.CreateClient(endpoint.ToString(), handler: requestHandler);
            client.DefaultRequestHeaders.Add(Constants.VersionHeaderName, MediaServiceVersion);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}