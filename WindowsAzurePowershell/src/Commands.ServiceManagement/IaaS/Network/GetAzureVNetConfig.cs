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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Net;
    using System.IO;
    using System.Management.Automation;
    using System.ServiceModel;
    using Commands.Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Model;
    using Properties;

    [Cmdlet(VerbsCommon.Get, "AzureVNetConfig"), OutputType(typeof(VirtualNetworkConfigContext))]
    public class GetAzureVNetConfigCommand : ServiceManagementBaseCmdlet
    {
        //public GetAzureVNetConfigCommand()
        //{
        //}

        //public GetAzureVNetConfigCommand(IServiceManagement channel)
        //{
        //    Channel = channel;
        //}

        [Parameter(HelpMessage = "The file path to save the network configuration to.")]
        [ValidateNotNullOrEmpty]
        public string ExportToFile
        {
            get;
            set;
        }

        public VirtualNetworkConfigContext GetVirtualNetworkConfigProcess()
        {
            this.ValidateParameters();

            VirtualNetworkConfigContext result = null;

            InvokeInOperationContext(() =>
            //using (new OperationContextScope(Channel.ToContextChannel()))
            {
                try
                {
                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetConfigBeginOperation, CommandRuntime.ToString()));

                    var netConfigStream = this.RetryCall(s => this.Channel.GetNetworkConfiguration(s)) as Stream;
                    Operation operation = GetOperation();

                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetConfigCompletedOperation, CommandRuntime.ToString()));

                    if (netConfigStream != null)
                    {
                        // TODO: might want to change this to an XML object of some kind...
                        var configReader = new StreamReader(netConfigStream);
                        var xml = configReader.ReadToEnd();

                        var networkConfig = new VirtualNetworkConfigContext
                        {
                            XMLConfiguration = xml,
                            OperationId = operation.OperationTrackingId,
                            OperationDescription = CommandRuntime.ToString(),
                            OperationStatus = operation.Status
                        };

                        if (!string.IsNullOrEmpty(this.ExportToFile))
                        {
                            networkConfig.ExportToFile(this.ExportToFile);
                        }

                        result = networkConfig;
                    }
                }
                catch (CloudException ex)
                //catch (ServiceManagementClientException ex)
                {
                    if (ex.Response.StatusCode == HttpStatusCode.NotFound && !IsVerbose())
                    //if (ex.HttpStatus == HttpStatusCode.NotFound && !IsVerbose())
                    {
                        result = null;
                    }
                    else
                    {
                        this.WriteExceptionDetails(ex);
                    }
                }
            });

            return result;
        }

        protected override void OnProcessRecord()
        {
            var networkConfig = this.GetVirtualNetworkConfigProcess();

            if (networkConfig != null)
            {
                WriteObject(networkConfig, true);
            }
        }

        private void ValidateParameters()
        {
            if (!string.IsNullOrEmpty(this.ExportToFile) && !Directory.Exists(Path.GetDirectoryName(this.ExportToFile)))
            {
                throw new ArgumentException(Resources.NetworkConfigurationDirectoryDoesNotExist);
            }
        }
    }
}