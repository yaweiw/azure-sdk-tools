﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.CloudService
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.ServiceBus;
    using ServiceManagement;

    [Cmdlet(VerbsDiagnostic.Test, "AzureName"), OutputType(typeof(bool))]
    public class TestAzureNameCommand : ServiceManagementBaseCmdlet
    {
        internal ServiceBusClientExtensions ServiceBusClient { get; set; }

        public TestAzureNameCommand()
        {
            
        }

        public TestAzureNameCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Service", HelpMessage = "Test for a cloud service name.")]
        public SwitchParameter Service
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Storage", HelpMessage = "Test for a storage account name.")]
        public SwitchParameter Storage
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "ServiceBusNamespace", HelpMessage = "Test for a service bus namespace name.")]
        public SwitchParameter ServiceBusNamespace
        {
            get;
            set;
        }

        [Parameter(Position = 1, ParameterSetName = "Service", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Cloud service name.")]
        [Parameter(Position = 1, ParameterSetName = "Storage", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Storage account name.")]
        [Parameter(Position = 1, ParameterSetName = "ServiceBusNamespace", Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service bus namespace name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        public AvailabilityResponse IsDNSAvailable(string subscriptionId, string name)
        {
            AvailabilityResponse result = Channel.IsDNSAvailable(subscriptionId, name);
            WriteObject(!result.Result);
            return result;
        }

        public AvailabilityResponse IsStorageServiceAvailable(string subscriptionId, string name)
        {
            AvailabilityResponse result = Channel.IsStorageServiceAvailable(subscriptionId, name);
            
            WriteObject(!result.Result);

            return result;
        }

        public bool IsServiceBusNamespaceAvailable(string subscriptionId, string name)
        {
            bool result = ServiceBusClient.IsAvailableNamespace(name);
            
            WriteObject(!result);

            return result;
        }

        public override void  ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (Service.IsPresent)
            {
                IsDNSAvailable(CurrentSubscription.SubscriptionId, Name);
            }
            else if (Storage.IsPresent)
            {
                IsStorageServiceAvailable(CurrentSubscription.SubscriptionId, Name);
            }
            else
            {
                ServiceBusClient = ServiceBusClient ?? new ServiceBusClientExtensions(CurrentSubscription);
                IsServiceBusNamespaceAvailable(CurrentSubscription.SubscriptionId, Name);
            }
        }
    }
}