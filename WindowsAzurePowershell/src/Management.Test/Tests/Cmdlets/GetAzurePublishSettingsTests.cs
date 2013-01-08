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

namespace Microsoft.WindowsAzure.Management.Test.Tests.Cmdlets
{
    using System;
    using Management.Cmdlets;
    using Management.Properties;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Utilities;

    [TestClass]
    public class GetAzurePublishSettingsTests
    {
        [TestMethod]
        public void GetAzurePublishSettingsProcessTest()
        {
            new GetAzurePublishSettingsCommand().GetAzurePublishSettingsProcess(General.PublishSettingsUrl);
        }

        /// <summary>
        /// Happy case, user has internet connection and uri specified is valid.
        /// </summary>
        [TestMethod]
        public void GetAzurePublishSettingsProcessTestFail()
        {
            Assert.IsFalse(string.IsNullOrEmpty(General.PublishSettingsUrl));
        }

        /// <summary>
        /// The url doesn't exist.
        /// </summary>
        [TestMethod]
        public void GetAzurePublishSettingsProcessTestEmptyDnsFail()
        {
            string emptyDns = string.Empty;
            string expectedMsg = string.Format(Resources.InvalidOrEmptyArgumentMessage, "publish settings url");
            
            try
            {
                new GetAzurePublishSettingsCommand().GetAzurePublishSettingsProcess(emptyDns);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
                Assert.IsTrue(string.Compare(expectedMsg, ex.Message, System.StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
    }
}