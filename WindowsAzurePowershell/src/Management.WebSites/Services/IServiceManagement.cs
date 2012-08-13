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

namespace Microsoft.WindowsAzure.Management.WebSites.Services
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel.Web;
    using System.ServiceModel;
    using Utilities;

    /// <summary>
    /// A list of hosted services
    /// </summary>
    [CollectionDataContract(Name = "Websites", ItemName = "Website", Namespace = Constants.ServiceManagementNS)]
    public class WebsiteList : List<Website>
    {
        public WebsiteList()
        {
        }

        public WebsiteList(IEnumerable<Website> websites)
            : base(websites)
        {
        }
    }

    /// <summary>
    /// A hosted service
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Website : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Provides the Windows Azure Service Management Api. 
    /// </summary>
    [ServiceContract(Namespace = Constants.ServiceManagementNS)]
    public partial interface IServiceManagement
    {
        /// <summary>
        /// Gets the properties for the specified hosted service.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/webspaces")]
        IAsyncResult BeginGetWebsite(string subscriptionId, AsyncCallback callback, object state);
        Website EndGetWebsite(IAsyncResult asyncResult);
    }
}
