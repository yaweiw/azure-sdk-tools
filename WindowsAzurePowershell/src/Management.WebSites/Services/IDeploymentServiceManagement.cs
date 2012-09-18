
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

namespace Microsoft.WindowsAzure.Management.Websites.Services
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using DeploymentEntities;

    /// <summary>
    /// Provides the Windows Azure Service Management Api for Windows Azure Websites Deployment. 
    /// </summary>
    [ServiceContract]
    public interface IDeploymentServiceManagement
    {
        [Description("Gets all deployments for a given repository")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "deployments?%24orderby=ReceivedTime%20desc&%24top={maxItems}")]
        IAsyncResult BeginGetDeployments(int maxItems, AsyncCallback callback, object state);
        Deployments EndGetDeployments(IAsyncResult asyncResult);
    }
}
