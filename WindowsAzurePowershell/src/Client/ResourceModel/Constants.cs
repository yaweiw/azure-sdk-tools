/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{

    public static class Constants
    {
        public const string ContinuationTokenHeaderName = "x-ms-continuation-token";
        public const string ClientRequestIdHeader = "x-ms-client-id";
        public const string OperationTrackingIdHeader = "x-ms-request-id";
        public const string PrincipalHeader = "x-ms-principal-id";
        public const string ServiceManagementNS = "http://schemas.microsoft.com/windowsazure";    
        public const string VersionHeaderName = "x-ms-version";
        public const string VersionHeaderContentLatest = "2013-03-01";
    }

    public static class DeploymentStatus
    {
        public const string Running = "Running";
        public const string Suspended = "Suspended";
        public const string RunningTransitioning = "RunningTransitioning";
        public const string SuspendedTransitioning = "SuspendedTransitioning";
        public const string Starting = "Starting";
        public const string Suspending = "Suspending";
        public const string Deploying = "Deploying";
        public const string Deleting = "Deleting";
        public const string Unavailable = "Unavailable";
    }

    public static class RoleInstanceStatus
    {
        public const string Initializing = "Initializing";
        public const string Ready = "Ready";
        public const string Busy = "Busy";
        public const string Stopping = "Stopping";
        public const string Stopped = "Stopped";
        public const string Unresponsive = "Unresponsive";

        public const string RoleStateUnknown = "RoleStateUnknown";
        public const string CreatingVM = "CreatingVM";
        public const string StartingVM = "StartingVM";
        public const string CreatingRole = "CreatingRole";
        public const string StartingRole = "StartingRole";
        public const string ReadyRole = "ReadyRole";
        public const string BusyRole = "BusyRole";

        public const string StoppingRole = "StoppingRole";
        public const string StoppingVM = "StoppingVM";
        public const string DeletingVM = "DeletingVM";
        public const string StoppedVM = "StoppedVM";
        public const string RestartingRole = "RestartingRole";
        public const string CyclingRole = "CyclingRole";

        public const string FailedStartingRole = "FailedStartingRole";
        public const string FailedStartingVM = "FailedStartingVM";
        public const string UnresponsiveRole = "UnresponsiveRole";

        public const string Provisioning = "Provisioning";
        public const string ProvisioningFailed = "ProvisioningFailed";
        public const string ProvisioningTimeout = "ProvisioningTimeout";
    }

    public static class OperationState
    {
        public const string InProgress = "InProgress";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
    }

    public static class KeyType
    {
        public const string Primary = "Primary";
        public const string Secondary = "Secondary";
    }

    public static class DeploymentSlotType
    {
        public const string Staging = "Staging";
        public const string Production = "Production";
    }

    public static class UpgradeType
    {
        public const string Auto = "Auto";
        public const string Manual = "Manual";
        public const string Simultaneous = "Simultaneous";
    }

    public static class CurrentUpgradeDomainState
    {
        public const string Before = "Before";
        public const string During = "During";
    }
}
