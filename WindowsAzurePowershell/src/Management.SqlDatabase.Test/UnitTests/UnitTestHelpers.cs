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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests
{
    using System;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Management.Model;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Common helper functions for SqlDatabase UnitTests.
    /// </summary>
    public class UnitTestHelpers
    {
        public static void CheckConfirmImpact(Type cmdlet, ConfirmImpact confirmImpact)
        {
            object[] cmdletAttributes = cmdlet.GetCustomAttributes(typeof(CmdletAttribute), true);
            Assert.AreEqual(1, cmdletAttributes.Length);
            CmdletAttribute attribute = (CmdletAttribute)cmdletAttributes[0];
            Assert.AreEqual(confirmImpact, attribute.ConfirmImpact);
        }

        public static void CheckCmdletModifiesData(Type cmdlet, bool supportsShouldProcess)
        {
            // If the Cmdlet modifies data, SupportsShouldProcess should be set to true.
            object[] cmdletAttributes = cmdlet.GetCustomAttributes(typeof(CmdletAttribute), true);
            Assert.AreEqual(1, cmdletAttributes.Length);
            CmdletAttribute attribute = (CmdletAttribute)cmdletAttributes[0];
            Assert.AreEqual(supportsShouldProcess, attribute.SupportsShouldProcess);

            if (supportsShouldProcess)
            {
                // If the Cmdlet modifies data, there needs to be a Force property to bypass ShouldProcess
                Assert.AreNotEqual(null, cmdlet.GetProperty("Force"), "Force property is expected for Cmdlets that modifies data.");
            }
        }

        public static SubscriptionData CreateUnitTestSubscription()
        {
            return new SubscriptionData()
            {
                SubscriptionName = "TestSubscription",
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                Certificate = new X509Certificate2()
            };
        }
    }
}
