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

namespace Microsoft.WindowsAzure.Commands.Websites.WebHostingPlan
{
    using Microsoft.WindowsAzure.Commands.Utilities.Websites;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Utilities.Properties;
    using Utilities.Websites.Common;
    using Utilities.Websites.Services;
    using Utilities.Websites.Services.DeploymentEntities;
    using Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebHostingPlan"), OutputType(typeof(SiteWithConfig), typeof(IEnumerable<WebHostingPlan>))]
    public class GetAzureWebHostingPlanCommand : WebHostingPlanContextBaseCmdlet
    {  
        public override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(WebSpaceName))
            {
                GetByName();
            }
            else if (!string.IsNullOrEmpty(WebSpaceName))
            {
                GetByWebSpace();
            }
            else
            {
                GetNoName();
            }
        }

        private void GetByName()
        {
            Do(() =>
            {
                var plan = WebsitesClient.GetWebHostingPlan(WebSpaceName, Name);
                WriteObject(plan, true);
            });
        }

        private void GetByWebSpace()
        {
            Do(() =>
            {
                var plan = WebsitesClient.ListWebHostingPlans(WebSpaceName);
                WriteObject(plan, true);
            });
        }

        private void GetNoName()
        {
            Do(() =>
                {
                    List<WebHostingPlan> plans = WebsitesClient.ListWebHostingPlans();
                    WriteObject(plans, true);
                });
        }

        private void Do(Action call)
        {
            try
            {
                call();
            }
            catch (CloudException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    WriteError(new ErrorRecord(new Exception(Resources.CommunicationCouldNotBeEstablished, ex), string.Empty, ErrorCategory.InvalidData, null));
                    throw;
                }
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                }
                throw;
            }
        }
    }
}
