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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void IsValidDirectoryPathReturnsTrueForExistingFolders()
        {
            Assert.IsTrue(FileUtils.IsValidDirectoryPath(Path.GetTempPath()));
            Assert.IsTrue(FileUtils.IsValidDirectoryPath(Directory.GetCurrentDirectory()));
            Assert.IsTrue(FileUtils.IsValidDirectoryPath("C:\\"));
        }

        [TestMethod]
        public void IsValidDirectoryPathReturnsFalseForFilePaths()
        {
            Assert.IsFalse(FileUtils.IsValidDirectoryPath(Path.GetTempPath() + "\\file.tst"));
            Assert.IsFalse(FileUtils.IsValidDirectoryPath(Path.GetTempPath() + "\\" + Guid.NewGuid() + "\\file.tst"));
            Assert.IsFalse(FileUtils.IsValidDirectoryPath("C:\\file.tst"));
        }

        [TestMethod]
        public void IsValidDirectoryPathReturnsFalseForNonExistingFolders()
        {
            Assert.IsFalse(FileUtils.IsValidDirectoryPath(""));
            Assert.IsFalse(FileUtils.IsValidDirectoryPath(null));
            Assert.IsFalse(FileUtils.IsValidDirectoryPath(Path.GetTempPath() + "\\" + Guid.NewGuid()));
            Assert.IsFalse(FileUtils.IsValidDirectoryPath("XYZ:\\"));
        }
    }
}
