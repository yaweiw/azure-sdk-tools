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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Commands.ResourceManager.Models;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test.Models
{
    public class ResourceManagerResourceIdentifierTests
    {
        [Fact]
        public void IdentifierIsConstructedFromProvidedValues()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier();
            id.Subscription = "abc123";
            id.ResourceGroupName = "group1";
            id.ResourceType = "Microsoft.Test/servers/db";
            id.ParentResource = "servers/r12345sql";
            id.ResourceName = "r45678db";

            Assert.Equal("/subscriptions/abc123/resourceGroups/group1/providers/Microsoft.Test/servers/r12345sql/db/r45678db", id.ToString());
        }

        [Fact]
        public void IdentifierIsConstructedWithoutParent()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier();
            id.Subscription = "abc123";
            id.ResourceGroupName = "group1";
            id.ResourceType = "Microsoft.Test/db";
            id.ResourceName = "r45678db";

            Assert.Equal("/subscriptions/abc123/resourceGroups/group1/providers/Microsoft.Test/db/r45678db", id.ToString());
        }

        [Fact]
        public void IdentifierIsConstructedWithMissingParameters()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier();

            Assert.Equal("/subscriptions//resourceGroups//providers///", id.ToString());
        }

        [Fact]
        public void IdentifierIsParsedFromShortId()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier("/subscriptions/abc123/resourceGroups/group1/providers/Microsoft.Test/db/r45678db");
            Assert.Equal("abc123", id.Subscription);
            Assert.Equal("group1", id.ResourceGroupName);
            Assert.Equal("Microsoft.Test/db", id.ResourceType);
            Assert.Null(id.ParentResource);
            Assert.Equal("r45678db", id.ResourceName);
        }

        [Fact]
        public void IdentifierIsParsedFromLongId()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier("/subscriptions/abc123/resourceGroups/group1/providers/Microsoft.Test/servers/r12345sql/db/r45678db");
            Assert.Equal("abc123", id.Subscription);
            Assert.Equal("group1", id.ResourceGroupName);
            Assert.Equal("Microsoft.Test/servers/db", id.ResourceType);
            Assert.Equal("servers/r12345sql", id.ParentResource);
            Assert.Equal("r45678db", id.ResourceName);
        }

        [Fact]
        public void IdentifierIsParsedFromVeryLongId()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier("/subscriptions/abc123/resourceGroups/group1/providers/Microsoft.Test/servers/r12345sql/subserver/r5555/db/r45678db");
            Assert.Equal("abc123", id.Subscription);
            Assert.Equal("group1", id.ResourceGroupName);
            Assert.Equal("Microsoft.Test/servers/subserver/db", id.ResourceType);
            Assert.Equal("servers/r12345sql/subserver/r5555", id.ParentResource);
            Assert.Equal("r45678db", id.ResourceName);
        }

        [Fact]
        public void IdentifierThrowsExceptionFromInvalidId()
        {
            Assert.Throws<ArgumentException>(()=> new ResourceManagerResourceIdentifier("/subscriptions/abc123/resourceGroups/group1"));
        }

        [Fact]
        public void IdentifierParsedIsSkippedWithEmptyId()
        {
            ResourceManagerResourceIdentifier id = new ResourceManagerResourceIdentifier(null);
            Assert.Null(id.Subscription);
            Assert.Null(id.ResourceGroupName);
            Assert.Null(id.ResourceType);
            Assert.Null(id.ParentResource);
            Assert.Null(id.ResourceName);

            id = new ResourceManagerResourceIdentifier("");
            Assert.Null(id.Subscription);
            Assert.Null(id.ResourceGroupName);
            Assert.Null(id.ResourceType);
            Assert.Null(id.ParentResource);
            Assert.Null(id.ResourceName);
        }
    }
}
