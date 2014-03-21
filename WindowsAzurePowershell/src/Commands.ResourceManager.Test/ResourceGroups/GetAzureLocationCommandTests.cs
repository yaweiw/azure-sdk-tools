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

using Microsoft.Azure.Commands.ResourceManager.Models;
using Moq;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test
{
    public class GetAzureLocationCommandTests
    {
        private GetAzureLocationCommand cmdlet;

        private Mock<ResourcesClient> resourcesClientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        public GetAzureLocationCommandTests()
        {
            resourcesClientMock = new Mock<ResourcesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetAzureLocationCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                ResourceClient = resourcesClientMock.Object
            };
        }

        [Fact]
        public void GetsLocations()
        {
            List<PSResourceProviderType> result = new List<PSResourceProviderType>()
            {
                new PSResourceProviderType()
                {
                    Locations = new List<string>() { "West US" },
                    Name = "Microsoft.Web"
                }
            };
            cmdlet.ResourceGroup = true;
            cmdlet.ResourceType = new string[] { "Microsoft.Web", "Microsoft.HDInsight" };
            resourcesClientMock.Setup(f => f.GetLocations("Microsoft.Web", "Microsoft.HDInsight", ResourcesClient.ResourceGroupTypeName)).Returns(result);

            cmdlet.ExecuteCmdlet();

            Assert.Equal(1, result.Count);

            commandRuntimeMock.Verify(f => f.WriteObject(result, true), Times.Once());
        }
    }
}
