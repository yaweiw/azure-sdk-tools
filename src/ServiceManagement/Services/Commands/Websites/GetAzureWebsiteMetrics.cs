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

using System.Data;

namespace Microsoft.WindowsAzure.Commands.Websites
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Websites.Common;
    using Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Commands.Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsiteMetrics"), OutputType(typeof(MetricResponse))]
    public class GetAzureWebsiteMetricsCommand : WebsiteContextBaseCmdlet
    {
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            HelpMessage = "List of metrics names to retrieve.")]
        [ValidateNotNullOrEmpty]
        public string[] MetricNames { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            HelpMessage = "The start time.")]
        [ValidateNotNullOrEmpty]
        public DateTime? StartDate { get; set; }

        [Parameter(Position = 4, Mandatory = false, ValueFromPipelineByPropertyName = true,
            HelpMessage = "The end time.")]
        public DateTime? EndDate { get; set; }

        public GetAzureWebsiteMetricsCommand()
        {
            websiteNameDiscovery = false;
        }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            var response = WebsitesClient.GetHistoricalUsageMetrics(Name, Slot, MetricNames, StartDate, EndDate);
            foreach (var metricResponse in response)
            {
                WriteObject(metricResponse, true);
            }
        }
    }
}
