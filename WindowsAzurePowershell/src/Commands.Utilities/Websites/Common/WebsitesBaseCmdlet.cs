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

namespace Microsoft.WindowsAzure.Commands.Utilities.Websites.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.Xml.Serialization;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using ServiceManagement;
    using Services;

    public abstract class WebsitesBaseCmdlet : CloudBaseCmdlet<IWebsitesServiceManagement>
    {
        protected override Operation WaitForOperation(string opdesc)
        {
            string operationId = RetrieveOperationId();
            Operation operation = new Operation();
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
                WebException webException = ex.InnerException as WebException;
                if (webException != null && webException.Response != null)
                {
                    using (StreamReader streamReader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof (ServiceError));
                        ServiceError serviceError = (ServiceError) serializer.Deserialize(streamReader);

                        string message;
                        if (serviceError.MessageTemplate.Equals(Resources.WebsiteAlreadyExists))
                        {
                            message = string.Format(Resources.WebsiteAlreadyExistsReplacement,
                                                            serviceError.Parameters.First());
                        }
                        else if (serviceError.MessageTemplate.Equals(Resources.CannotFind) &&
                                 ("WebSpace".Equals(serviceError.Parameters.FirstOrDefault()) ||
                                 "GeoRegion".Equals(serviceError.Parameters.FirstOrDefault())))
                        {
                            message = string.Format(Resources.CannotFind, "Location",
                                                            serviceError.Parameters[1]);
                        }
                        else
                        {
                            message = serviceError.Message;
                        }

                        if (showError)
                        {
                            WriteExceptionError(new Exception(serviceError.Message));
                        }

                        return message;
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
