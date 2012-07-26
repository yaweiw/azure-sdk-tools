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

using System;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.CloudService.Test;
using Microsoft.WindowsAzure.Management.SqlDatabase.Firewall.Cmdlet;
using Microsoft.WindowsAzure.Management.SqlDatabase.Server.Cmdlet;

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Server.Cmdlet
{
    /// <summary>
    /// These tests prevent regression in parameter validation attributes.
    /// </summary>
    [TestClass]
    public class ServerCmdletAttributionTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
        }

        [TestMethod]
        public void GetAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(GetAzureSqlDatabaseServer);
            UnitTestHelpers.CheckConfirmImpact(cmdlet, ConfirmImpact.None);
            UnitTestHelpers.CheckCmdletModifiesData(cmdlet, false);
        }

        [TestMethod]
        public void NewAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(NewAzureSqlDatabaseServer);
            UnitTestHelpers.CheckConfirmImpact(cmdlet, ConfirmImpact.Low);
            UnitTestHelpers.CheckCmdletModifiesData(cmdlet, true);
        }

        [TestMethod]
        public void RemoveAzureSqlDatabaseServerAttributeTest()
        {
            Type cmdlet = typeof(RemoveAzureSqlDatabaseServer);
            UnitTestHelpers.CheckConfirmImpact(cmdlet, ConfirmImpact.High);
            UnitTestHelpers.CheckCmdletModifiesData(cmdlet, true);
        }

        [TestMethod]
        public void SetAzureSqlDatabaseServerAdminPasswordAttributeTest()
        {
            Type cmdlet = typeof(SetAzureSqlDatabaseServer);
            UnitTestHelpers.CheckConfirmImpact(cmdlet, ConfirmImpact.Medium);
            UnitTestHelpers.CheckCmdletModifiesData(cmdlet, true);
        }
    }
}
