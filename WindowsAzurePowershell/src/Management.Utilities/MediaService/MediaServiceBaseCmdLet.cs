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
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.Management.Utilities.Common;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services;
using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Management.Utilities.MediaService
{
    public abstract class MediaServiceBaseCmdlet : CloudBaseCmdlet<IMediaServiceManagement>
    {
        protected override Operation WaitForOperation(string opdesc)
        {
            string operationId = RetrieveOperationId();
            var operation = new Operation();
            operation.OperationTrackingId = operationId;
            operation.Status = "Success";
            return operation;
        }

        protected string ProcessException(Exception ex)
        {
            return ProcessException(ex, true);
        }

        protected string ProcessException(Exception ex, bool showError)
        {
            if (ex.InnerException is WebException)
            {
                var webException = ex.InnerException as WebException;
                if (webException != null && webException.Response != null)
                {
                    using (var streamReader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        var serializer = new XmlSerializer(typeof (ServiceError));
                        var serviceError = (ServiceError) serializer.Deserialize(streamReader);

                        if (showError)
                        {
                            WriteExceptionError(new Exception(serviceError.Message));
                        }

                        return serviceError.Message;
                    }
                }
            }

            if (showError)
            {
                WriteExceptionError(ex);
            }

            return ex.Message;
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
            }
            catch (EndpointNotFoundException ex)
            {
                ProcessException(ex);
            }
            catch (ProtocolException ex)
            {
                ProcessException(ex);
            }
        }
    }
}