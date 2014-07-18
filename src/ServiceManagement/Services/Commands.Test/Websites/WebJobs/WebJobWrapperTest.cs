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

namespace Microsoft.WindowsAzure.Commands.Test.Websites
{
    using System;
    using System.ComponentModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Websites.WebJobs;
    using Microsoft.WindowsAzure.WebSitesExtensions.Models;

    [TestClass]
    public class WebJobWrapperTest
    {
        [TestMethod]
        public void ReadProperties_ValuesAreSameWiththeInternalWebJobInstance()
        {
            //Set up
            TriggeredWebJobRun jobRun = new TriggeredWebJobRun();
            TriggeredWebJob webJob = new TriggeredWebJob()
            {
                ExtraInfoUrl = "an extra info url",
                HistoryUrl = "a history url",
                LatestRun = jobRun,
                Name = "my web job name",
                Type = WebJobType.Triggered,
                RunCommand = "my run command",
                Url = new System.Uri("http://myWebJobUrl")
            };

            // Test
            PSTriggeredWebJob wrapper = new PSTriggeredWebJob(webJob);

            // Assert
            Assert.AreEqual(webJob.ExtraInfoUrl, wrapper.ExtraInfoUrl);
            Assert.AreEqual(webJob.HistoryUrl, wrapper.HistoryUrl);
            Assert.AreEqual(webJob.LatestRun, wrapper.LatestRun);
            Assert.AreEqual(webJob.Name, wrapper.JobName);
            Assert.AreEqual(webJob.RunCommand, wrapper.RunCommand);
            Assert.AreEqual(webJob.Type, wrapper.JobType);
            Assert.AreEqual(webJob.Url, wrapper.Url);
        }

        [TestMethod]
        public void WriteProperties_InternalWebJobInstanceIsUpdated()
        {
            //Set up
            TriggeredWebJobRun jobRun = new TriggeredWebJobRun();
            TriggeredWebJob webJob = new TriggeredWebJob()
            {
                Type = WebJobType.Triggered,
            };

            string jobName = "My Job Name";
            WebJobType jobType = WebJobType.Triggered;
            string detailedStatus = "some details";
            string extraInfoUrl = "an extra info url";
            string historyUrl = "a history url";
            TriggeredWebJobRun latestRun = new TriggeredWebJobRun();
            string logUrl = "a log url";
            string status = "my web job status";
            string runCommand = "my run command";
            Uri url = new System.Uri("http://myWebJobUrl");

            // Test
            PSTriggeredWebJob wrapper = new PSTriggeredWebJob(webJob);
            wrapper.JobType = jobType;
            wrapper.JobName = jobName;
            wrapper.ExtraInfoUrl = extraInfoUrl;
            wrapper.HistoryUrl = historyUrl;
            wrapper.LatestRun = latestRun;
            wrapper.RunCommand = runCommand;
            wrapper.Url = url;

            // Assert
            Assert.AreEqual(jobName, wrapper.JobName);
            Assert.AreEqual(jobType, wrapper.JobType);
            Assert.AreEqual(extraInfoUrl, wrapper.ExtraInfoUrl);
            Assert.AreEqual(historyUrl, wrapper.HistoryUrl);
            Assert.AreEqual(latestRun, wrapper.LatestRun);
            Assert.AreEqual(runCommand, wrapper.RunCommand);
            Assert.AreEqual(url, wrapper.Url);
            Assert.AreEqual(jobName, webJob.Name);
            Assert.AreEqual(jobType, webJob.Type);
            Assert.AreEqual(extraInfoUrl, webJob.ExtraInfoUrl);
            Assert.AreEqual(historyUrl, webJob.HistoryUrl);
            Assert.AreEqual(latestRun, webJob.LatestRun);
            Assert.AreEqual(runCommand, webJob.RunCommand);
            Assert.AreEqual(url, webJob.Url);
        }

        [TestMethod]
        public void SamePropertyNumberWithTriggeredWebJobModelClass()
        {
            // Setup & Test
            int webJobPropertyNumber = TypeDescriptor.GetProperties(typeof(TriggeredWebJob)).Count - 1; // Ignore the error property
            int webJobWrapperPropertyNumber = TypeDescriptor.GetProperties(typeof(PSTriggeredWebJob)).Count;

            // Assert
            Assert.AreEqual(webJobPropertyNumber, webJobWrapperPropertyNumber,
            "\'TriggeredWebJob\' class has properties add/removed, please update \'WebJobWrapper\' class");
        }

        [TestMethod]
        public void SamePropertyNumberWithContinuousWebJobModelClass()
        {
            // Setup & Test
            int webJobPropertyNumber = TypeDescriptor.GetProperties(typeof(ContinuousWebJob)).Count - 1; // Ignore the error property
            int webJobWrapperPropertyNumber = TypeDescriptor.GetProperties(typeof(PSContinuousWebJob)).Count;

            // Assert
            Assert.AreEqual(webJobPropertyNumber, webJobWrapperPropertyNumber,
            "\'ContinuousWebJob\' class has properties add/removed, please update \'WebJobWrapper\' class");
        }
    }
}
