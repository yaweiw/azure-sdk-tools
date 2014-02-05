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
            WebJobRun jobRun = new WebJobRun();
            WebJob webJob = new WebJob()
            {
                DetailedStatus = "some details",
                ExtraInfoUrl = "an extra info url",
                HistoryUrl = "a history url",
                LatestRun = jobRun,
                LogUrl = "a log url",
                Name = "my web job name",
                Status = "my web job status",
                Type = WebJobType.Triggered,
                RunCommand = "my run command",
                Url = new System.Uri("http://myWebJobUrl")
            };

            // Test
            WebJobWrapper wrapper = new WebJobWrapper(webJob);

            // Assert
            Assert.AreEqual(webJob.DetailedStatus, wrapper.DetailedStatus);
            Assert.AreEqual(webJob.ExtraInfoUrl, wrapper.ExtraInfoUrl);
            Assert.AreEqual(webJob.HistoryUrl, wrapper.HistoryUrl);
            Assert.AreEqual(webJob.LatestRun, wrapper.LatestRun);
            Assert.AreEqual(webJob.LogUrl, wrapper.LogUrl);
            Assert.AreEqual(webJob.Name, wrapper.JobName);
            Assert.AreEqual(webJob.RunCommand, wrapper.RunCommand);
            Assert.AreEqual(webJob.Status, wrapper.Status);
            Assert.AreEqual(webJob.Type, wrapper.JobType);
            Assert.AreEqual(webJob.Url, wrapper.Url);
        }

        [TestMethod]
        public void WriteProperties_InternalWebJobInstanceIsUpdated()
        {
            //Set up
            WebJobRun jobRun = new WebJobRun();
            WebJob webJob = new WebJob()
            {
                Type = WebJobType.Triggered,
            };

            string newJobName = "My Job Name";
            WebJobType newJobType = WebJobType.Triggered;

            // Test
            WebJobWrapper wrapper = new WebJobWrapper(webJob);
            wrapper.JobType = newJobType;
            wrapper.JobName = newJobName;

            // Assert
            Assert.AreEqual(newJobName, wrapper.JobName);
            Assert.AreEqual(newJobName, webJob.Name);
            Assert.AreEqual(newJobType, wrapper.JobType);
            Assert.AreEqual(newJobType, webJob.Type);
        }

        [TestMethod]
        public void SamePropertyNumberWithWebJobModelClass()
        {
            // Setup & Test
            int webJobPropertyNumber = TypeDescriptor.GetProperties(typeof(WebJob)).Count;
            int webJobWrapperPropertyNumber = TypeDescriptor.GetProperties(typeof(WebJobWrapper)).Count;

            // Assert
            Assert.AreEqual(webJobPropertyNumber, webJobWrapperPropertyNumber,
            "\'WebJob\' class has properties add/removed, please update \'WebJobWrapper\' class");
        }
    }
}
