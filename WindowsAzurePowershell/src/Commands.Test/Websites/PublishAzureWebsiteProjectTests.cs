using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.Deployment;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Websites;
using Microsoft.WindowsAzure.Commands.Utilities.Websites;
using Microsoft.WindowsAzure.Commands.Websites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.Test.Websites
{
    [TestClass]
    public class PublishAzureWebsiteProjectTests : WebsitesTestBase
    {
        [TestMethod]
        public void PublishFromPackage()
        {
            var websiteName = "test-site";
            string slot = null;
            var package = "test-package";
            var connectionStrings = new Hashtable();
            connectionStrings["DefaultConnection"] = "test-connection-string";

            var publishProfile = new WebSiteGetPublishProfileResponse.PublishProfile()
            {
                UserName = "test-user-name",
                UserPassword = "test-password",
                PublishUrl = "test-publlish-url"
            };

            var published = false;

            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();

            clientMock.Setup(c => c.GetWebDeployPublishProfile(websiteName, slot)).Returns(publishProfile);
            clientMock.Setup(c => c.PublishWebProject(websiteName, slot, package, connectionStrings))
                .Callback((string n, string s, string p, Hashtable cs) =>
                {
                    Assert.AreEqual(websiteName, n);
                    Assert.AreEqual(slot, s);
                    Assert.AreEqual(package, p);
                    Assert.AreEqual(connectionStrings, cs);
                    published = true;
                });

            Mock<ICommandRuntime> powerShellMock = new Mock<ICommandRuntime>();

            var command = new PublishAzureWebsiteProject()
            {
                CommandRuntime = powerShellMock.Object,
                Name = websiteName,
                Package = package,
                ConnectionString = connectionStrings,
                WebsitesClient = clientMock.Object
            };

            command.ExecuteCmdlet();

            powerShellMock.Verify(f => f.WriteVerbose(string.Format("[Complete] Publishing package {0}", package)), Times.Once());
            Assert.IsTrue(published);
        }

        [TestMethod]
        public void PublishFromProjectFile()
        {
            var websiteName = "test-site";
            string slot = null;
            var projectFile = "WebApplication4.csproj";
            var configuration = "Debug";
            var logFile = "build.log";
            var connectionStrings = new Hashtable();
            connectionStrings["DefaultConnection"] = "test-connection-string";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string originalDirectory = Directory.GetCurrentDirectory();
            }

            var publishProfile = new WebSiteGetPublishProfileResponse.PublishProfile()
            {
                UserName = "test-user-name",
                UserPassword = "test-password",
                PublishUrl = "test-publlish-url"
            };
            var package = "test-package.zip";

            var published = false;

            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();

            clientMock.Setup(c => c.GetWebDeployPublishProfile(websiteName, slot)).Returns(publishProfile);
            clientMock.Setup(c => c.PublishWebProject(websiteName, slot, package, connectionStrings))
                .Callback((string n, string s, string p, Hashtable cs) =>
                {
                    Assert.AreEqual(websiteName, n);
                    Assert.AreEqual(slot, s);
                    Assert.AreEqual(package, p);
                    Assert.AreEqual(connectionStrings, cs);
                    published = true;
                });

            Mock<ICommandRuntime> powerShellMock = new Mock<ICommandRuntime>();

            var command = new PublishAzureWebsiteProject()
            {
                CommandRuntime = powerShellMock.Object,
                WebsitesClient = clientMock.Object,
                Name = websiteName,
                ProjectFile = projectFile,
                Configuration = configuration,
                ConnectionString = connectionStrings
            };

            command.ExecuteCmdlet();

            powerShellMock.Verify(f => f.WriteVerbose(string.Format("[Complete] Publishing package {0}", package)), Times.Once());
            Assert.IsTrue(published);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void PublishProjectFileNotExist()
        {
            var websiteName = "test-site";
            var projectFile = "file-not-exist.csproj";
            var configuration = "Debug";

            var publishProfile = new WebSiteGetPublishProfileResponse.PublishProfile()
            {
                UserName = "test-user-name",
                UserPassword = "test-password",
                PublishUrl = "test-publlish-url"
            };

            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();

            Mock<ICommandRuntime> powerShellMock = new Mock<ICommandRuntime>();

            var command = new PublishAzureWebsiteProject()
            {
                CommandRuntime = powerShellMock.Object,
                WebsitesClient = clientMock.Object,
                Name = websiteName,
                ProjectFile = projectFile,
                Configuration = configuration
            };

            command.ExecuteCmdlet();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void PublishWebConfigFileNotExist()
        {
            var websiteName = "test-site";
            var projectFile = "WebApplication4.csproj";
            var configuration = "not-exist";

            var publishProfile = new WebSiteGetPublishProfileResponse.PublishProfile()
            {
                UserName = "test-user-name",
                UserPassword = "test-password",
                PublishUrl = "test-publlish-url"
            };

            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();

            Mock<ICommandRuntime> powerShellMock = new Mock<ICommandRuntime>();

            var command = new PublishAzureWebsiteProject()
            {
                CommandRuntime = powerShellMock.Object,
                WebsitesClient = clientMock.Object,
                Name = websiteName,
                ProjectFile = projectFile,
                Configuration = configuration
            };

            command.ExecuteCmdlet();
        }
    }
}
