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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Utilities
{
    using CloudService.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CloudServiceCmdletTestBase : TestBase
    {
        protected MockCommandRuntime mockCommandRuntime;

        protected SimpleServiceManagement channel;

        protected EnableAzureMemcacheRoleCommand enableCacheCmdlet;

        protected NewAzureServiceProjectCommand newServiceCmdlet;

        protected AddAzureNodeWebRoleCommand addNodeWebCmdlet;

        protected AddAzureNodeWorkerRoleCommand addNodeWorkerCmdlet;

        protected AddAzureCacheWorkerRoleCommand addCacheRoleCmdlet;

        protected DisableAzureServiceProjectRemoteDesktopCommand disableRDCmdlet;

        protected SetAzureServiceProjectCommand setServiceProjectCmdlet;

        protected StartAzureServiceCommand startServiceCmdlet;

        protected StopAzureServiceCommand stopServiceCmdlet;

        protected RemoveAzureServiceCommand removeServiceCmdlet;

        protected PublishAzureServiceProjectCommand publishServiceCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();
            channel = new SimpleServiceManagement();
            
            enableCacheCmdlet = new EnableAzureMemcacheRoleCommand();
            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addNodeWebCmdlet = new AddAzureNodeWebRoleCommand();
            addNodeWorkerCmdlet = new AddAzureNodeWorkerRoleCommand();
            addCacheRoleCmdlet = new AddAzureCacheWorkerRoleCommand();
            disableRDCmdlet = new DisableAzureServiceProjectRemoteDesktopCommand();
            setServiceProjectCmdlet = new SetAzureServiceProjectCommand();
            startServiceCmdlet = new StartAzureServiceCommand(channel) { ShareChannel = true };
            stopServiceCmdlet = new StopAzureServiceCommand(channel) { ShareChannel = true };
            removeServiceCmdlet = new RemoveAzureServiceCommand(channel) { ShareChannel = true };
            publishServiceCmdlet = new PublishAzureServiceProjectCommand(channel) { ShareChannel = true };
            
            disableRDCmdlet.CommandRuntime = mockCommandRuntime;
            addCacheRoleCmdlet.CommandRuntime = mockCommandRuntime;
            addNodeWorkerCmdlet.CommandRuntime = mockCommandRuntime;
            addNodeWebCmdlet.CommandRuntime = mockCommandRuntime;
            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
            enableCacheCmdlet.CommandRuntime = mockCommandRuntime;
            setServiceProjectCmdlet.CommandRuntime = mockCommandRuntime;
            startServiceCmdlet.CommandRuntime = mockCommandRuntime;
            stopServiceCmdlet.CommandRuntime = mockCommandRuntime;
            removeServiceCmdlet.CommandRuntime = mockCommandRuntime;
            publishServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }
    }
}
