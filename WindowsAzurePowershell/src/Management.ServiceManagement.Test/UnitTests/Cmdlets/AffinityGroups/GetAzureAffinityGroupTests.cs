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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.UnitTests.Cmdlets.AffinityGroups
{
    using System.Collections;
    using System.Linq;
    using System.Management.Automation;
    using Extensions;
    using Management.Test.Stubs;
    using Samples.WindowsAzure.ServiceManagement;
    using ServiceManagement.AffinityGroups;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class GetAzureAffinityGroupTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void GetAzureAffinityGroupSingleTest()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            channel.GetAffinityGroupThunk = ar => new AffinityGroup { Name = "affinity1" };

            // Test
            GetAzureAffinityGroup getAzureAffinityGroupCommand = new GetAzureAffinityGroup(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime()
            };

            getAzureAffinityGroupCommand.Name = "affinity1";
            getAzureAffinityGroupCommand.ExecuteCommand();

            Assert.AreEqual(1, ((MockCommandRuntime)getAzureAffinityGroupCommand.CommandRuntime).OutputPipeline.Count);
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(((MockCommandRuntime)getAzureAffinityGroupCommand.CommandRuntime).OutputPipeline);
            Assert.IsNotNull(enumerator);

            enumerator.MoveNext();
            Assert.IsTrue(((AffinityGroup)enumerator.Current).Name.Equals("affinity1"));
        }

        [TestMethod]
        public void GetAzureAffinityGroupMultipleTest()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            channel.ListAffinityGroupsThunk = ar => new AffinityGroupList(new[] { new AffinityGroup { Name = "affinity2" }, new AffinityGroup { Name = "affinity3" } });

            // Test
            GetAzureAffinityGroup getAzureAffinityGroupCommand = new GetAzureAffinityGroup(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime()
            };

            getAzureAffinityGroupCommand.ExecuteCommand();

            Assert.AreEqual(1, ((MockCommandRuntime)getAzureAffinityGroupCommand.CommandRuntime).OutputPipeline.Count);
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(((MockCommandRuntime)getAzureAffinityGroupCommand.CommandRuntime).OutputPipeline.First());
            Assert.IsNotNull(enumerator);

            enumerator.MoveNext();
            Assert.IsTrue(((AffinityGroup)enumerator.Current).Name.Equals("affinity2"));

            enumerator.MoveNext();
            Assert.IsTrue(((AffinityGroup)enumerator.Current).Name.Equals("affinity3"));
        }
    }
}