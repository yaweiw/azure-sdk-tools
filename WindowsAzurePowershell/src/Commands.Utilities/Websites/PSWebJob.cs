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

namespace Microsoft.WindowsAzure.Commands.Websites.WebJobs
{
    using System;
    using Microsoft.WindowsAzure.WebSitesExtensions.Models;

    /// <summary>
    /// The purpose of the wrapping is to surface a Web Job's "Name" property as "JobName",
    /// and "Type" as "JobType". This is needed for PowerShell pipeline.
    /// </summary>
    public class PSWebJob
    {
        private WebJob _webJob;

        public PSWebJob(WebJob webJob)
        {
            _webJob = webJob;
        }

        public PSWebJob()
        {
            _webJob = new WebJob();
        }

        public WebJobType JobType
        {
            get
            {
                return _webJob.Type;
            }
            set
            {
                _webJob.Type = value;
            }
        }

        public string JobName
        {
            get
            {
                return _webJob.Name;
            }
            set
            {
                _webJob.Name = value;
            }
        }

        public string DetailedStatus
        {
            get
            {
                return _webJob.DetailedStatus;
            }
            set
            {
                _webJob.DetailedStatus = value;
            }
        }

        public string ExtraInfoUrl
        {
            get
            {
                return _webJob.ExtraInfoUrl;
            }
            set
            {
                _webJob.ExtraInfoUrl = value;
            }
        }

        public string HistoryUrl
        {
            get
            {
                return _webJob.HistoryUrl;
            }
            set
            {
                _webJob.HistoryUrl = value;
            }
        }

        public WebJobRun LatestRun
        {
            get
            {
                return _webJob.LatestRun;
            }
            set
            {
                _webJob.LatestRun = value;
            }
        }

        public string LogUrl
        {
            get
            {
                return _webJob.LogUrl;
            }
            set
            {
                _webJob.LogUrl = value;
            }
        }

        public string RunCommand 
        { 
            get
            {
                return _webJob.RunCommand;
            }
            set
            {
                _webJob.RunCommand = value;
            }
        }

        public string Status 
        { 
            get
            {
                return _webJob.Status;
            }
            set
            {
                _webJob.Status = value;
            }
        }

        public Uri Url 
        {
            get
            {
                return _webJob.Url;
            }
            set
            {
                _webJob.Url = value;
            }
        }
    }
}
