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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets.Common
{
    using System;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.Xml.Serialization;
    using Management.Cmdlets.Common;
    using Samples.WindowsAzure.ServiceManagement;
    using Services;

    public abstract class WebsitesCmdletBase : CloudBaseCmdlet<IWebsitesServiceManagement>
    {
        protected override Operation WaitForOperation(string opdesc)
        {
            string operationId = RetrieveOperationId();
            Operation operation = new Operation();
            operation.OperationTrackingId = operationId;
            operation.Status = "Success";
            return operation;
        }

        internal abstract bool ExecuteCommand();

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                // Execute actual cmdlet action
                ExecuteCommand();
            }
            catch (ProtocolException ex)
            {
                if (ex.InnerException is WebException)
                {
                    StreamReader streamReader = new StreamReader(((WebException)ex.InnerException).Response.GetResponseStream());
                    XmlSerializer serializer = new XmlSerializer(typeof(ServiceError));
                    ServiceError serviceError = (ServiceError)serializer.Deserialize(streamReader);
                    SafeWriteError(new Exception(serviceError.Message));
                }
                else
                {
                    SafeWriteError(ex);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }
    }
}
