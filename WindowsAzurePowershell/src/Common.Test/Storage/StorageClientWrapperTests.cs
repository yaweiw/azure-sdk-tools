using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Management.Storage;
using Moq;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Storage
{
    [TestClass]
    public class StorageClientWrapperTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Commands.Test.Utilities.Common.Data.AzureSdkAppDir;
        }

        [TestMethod]
        public void TestUpload()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var subscription = WindowsAzureProfile.Instance.CurrentSubscription;
                var storageManagementClient = subscription.CreateClient<StorageManagementClient>();
                var storageClientWrapper = new StorageClientWrapper(storageManagementClient);
                var templateFilePath = storageClientWrapper.UploadFileToBlob(new BlobUploadParameters
                {
                    StorageName = "test",
                    FileLocalPath = "D:\\Code\\test-template.js",
                    FileRemoteName = Path.GetFileNameWithoutExtension("test-template.js"),
                    OverrideIfExists = true,
                    ContainerPublic = true,
                    ContainerName = "test"
                });

                Assert.IsNotNull(templateFilePath);
            }
        }
    }
}
