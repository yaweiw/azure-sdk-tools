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

namespace Microsoft.WindowsAzure.Management.WebSites.Cmdlets.Common
{
    using System;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using Websites.Cmdlets.Common;
    using Websites.Services;

    public abstract class WebsiteContextCmdletBase : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(Name))
                {
                    // If the website name was not specified as a parameter try to infer it
                    Name = GitWebsite.ReadConfiguration().Name;
                }

                base.ProcessRecord();
            }
            catch (ProtocolException ex)
            {
                if (ex.InnerException is WebException)
                {
                    SafeWriteError(ex.InnerException);
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
