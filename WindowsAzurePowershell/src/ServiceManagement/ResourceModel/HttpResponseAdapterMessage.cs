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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Data.OData;
    using System.ServiceModel.Channels;

    internal class HttpResponseAdapterMessage : IODataResponseMessage
    {
        private HttpResponseMessageProperty resposeProperties = null;
        private Stream responseStream = null;

        public HttpResponseAdapterMessage(Message reply, Stream responseStream)
        {
            this.resposeProperties = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            this.responseStream = responseStream;
        }

        public Task<Stream> GetStreamAsync()
        {
            return Task.Factory.StartNew(() => this.responseStream);
        }

        public string GetHeader(string headerName)
        {
            return resposeProperties.Headers[headerName];
        }

        public Stream GetStream()
        {
            return this.responseStream;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                List<KeyValuePair<string, string>> retHeaders = new List<KeyValuePair<string, string>>();

                foreach (string key in this.resposeProperties.Headers.Keys)
                {
                    retHeaders.Add(new KeyValuePair<string, string>(key, this.resposeProperties.Headers[key]));
                }

                return retHeaders;
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            throw new NotImplementedException();
        }

        public int StatusCode
        {
            get
            {
                return (int)this.resposeProperties.StatusCode;
            }

            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
