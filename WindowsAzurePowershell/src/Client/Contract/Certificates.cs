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
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public partial interface IServiceManagement
    {
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/certificates")]
        IAsyncResult BeginListCertificates(string subscriptionId, string serviceName, AsyncCallback callback, object state);
        CertificateList EndListCertificates(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/certificates/{thumbprintalgorithm}-{thumbprint_in_hex}")]
        IAsyncResult BeginGetCertificate(string subscriptionId, string serviceName, string thumbprintalgorithm, string thumbprint_in_hex, AsyncCallback callback, object state);
        Certificate EndGetCertificate(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/certificates")]
        IAsyncResult BeginAddCertificates(string subscriptionId, string serviceName, CertificateFile input, AsyncCallback callback, object state);
        void EndAddCertificates(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/certificates/{thumbprintalgorithm}-{thumbprint_in_hex}")]
        IAsyncResult BeginDeleteCertificate(string subscriptionId, string serviceName, string thumbprintalgorithm, string thumbprint_in_hex, AsyncCallback callback, object state);
        void EndDeleteCertificate(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static CertificateList ListCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndListCertificates(proxy.BeginListCertificates(subscriptionId, serviceName, null, null));
        }

        public static Certificate GetCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
        {
            return proxy.EndGetCertificate(proxy.BeginGetCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
        }

        public static void AddCertificates(this IServiceManagement proxy, string subscriptionId, string serviceName, CertificateFile input)
        {
            proxy.EndAddCertificates(proxy.BeginAddCertificates(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteCertificate(this IServiceManagement proxy, string subscriptionId, string serviceName, string algorithm, string thumbprint)
        {
            proxy.EndDeleteCertificate(proxy.BeginDeleteCertificate(subscriptionId, serviceName, algorithm, thumbprint, null, null));
        }
    }
}
