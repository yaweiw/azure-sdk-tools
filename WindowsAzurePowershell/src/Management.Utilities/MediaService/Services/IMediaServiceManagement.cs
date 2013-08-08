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
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;

namespace Microsoft.WindowsAzure.Management.Utilities.MediaService.Services
{
    /// <summary>
    ///     Provides the Windows Azure Service Management Api for Windows Azure Websites.
    /// </summary>
    [ServiceContract(Namespace = MediaServicesUriElements.ServiceNamespace)]
    [ServiceKnownType(typeof (MediaServiceAccount))]
    [ServiceKnownType(typeof (MediaServiceAccountDetails))]
    [ServiceKnownType(typeof (AccountKeys))]
    public interface IMediaServiceManagement
    {
        #region Site CRUD

        [Description("Returns all the mediaservices for a given subscription.")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = MediaServicesUriElements.MediaServiceRoot)]
        IAsyncResult BeginGetMediaServices(string subscriptionId, AsyncCallback callback, object state);

        MediaServiceAccounts EndGetMediaServices(IAsyncResult asyncResult);


        [Description("Returns a mediaservices by a name for a given subscription.")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = MediaServicesUriElements.MediaServiceAccountDetails)]
        IAsyncResult BeginGetMediaService(string subscriptionId, string name, AsyncCallback callback, object state);

        MediaServiceAccountDetails EndGetMediaService(IAsyncResult asyncResult);

        [Description("Deletes the account for a given subscription.")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = MediaServicesUriElements.MediaServiceRoot + "/{accountName}")]
        IAsyncResult BeginDeleteMediaServicesAccount(string subscriptionId, string accountName, AsyncCallback callback, object state);

        void EndDeleteMediaServicesAccount(IAsyncResult asyncResult);

        [Description("Regenerates an account for a given subscription.")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = MediaServicesUriElements.MediaServiceRoot + "/{accountName}/AccountKeys/{keyType}/Regenerate")]
        IAsyncResult BeginRegenerateMediaServicesAccount(string subscriptionId, string accountName, string keyType, AsyncCallback callback, object state);

        void EndRegenerateMediaServicesAccount(IAsyncResult asyncResult);

        #endregion
    }
}