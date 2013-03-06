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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// The deployment-specific interface of the resource model service.
    /// </summary>
    public partial interface IServiceManagement
    {
        #region Swap Deployment

        /// <summary>
        /// Swaps the deployment to a production slot.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}")]
        IAsyncResult BeginSwapDeployment(string subscriptionId, string serviceName, SwapDeploymentInput input, AsyncCallback callback, object state);

        void EndSwapDeployment(IAsyncResult asyncResult);

        #endregion

        #region Create Deployment

        /// <summary>
        /// Creates a deployment.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
        IAsyncResult BeginCreateOrUpdateDeployment(string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input, AsyncCallback callback, object state);

        void EndCreateOrUpdateDeployment(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments")]
        IAsyncResult BeginCreateDeployment(string subscriptionId, string serviceName, Deployment deployment, AsyncCallback callback, object state);

        void EndCreateDeployment(IAsyncResult asyncResult);
        #endregion

        #region Delete Deployment

        /// <summary>
        /// Deletes the specified deployment. This works against either through the slot or through the name.This is an asynchronous operation.
        /// Only implements deleting by deployment name right now. 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}")]
        IAsyncResult BeginDeleteDeployment(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

        void EndDeleteDeployment(IAsyncResult asyncResult);

        /// <summary>
        /// Deletes the specified deployment. This works against either through the slot or through the name.This is an asynchronous operation.
        /// Only implements deleting by deployment name right now. 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
        IAsyncResult BeginDeleteDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

        void EndDeleteDeploymentBySlot(IAsyncResult asyncResult);

        #endregion

        #region Get Deployment

        /// <summary>
        /// Gets the specified deployment details.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}")]
        IAsyncResult BeginGetDeployment(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

        Deployment EndGetDeployment(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the specified deployment details.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}")]
        IAsyncResult BeginGetDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

        Deployment EndGetDeploymentBySlot(IAsyncResult asyncResult);

        #endregion

        #region Change Deployment Config

        /// <summary>
        /// Initiates a change to the deployment. This works against through the deployment name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=config")]
        IAsyncResult BeginChangeConfiguration(string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input, AsyncCallback callback, object state);

        void EndChangeConfiguration(IAsyncResult asyncResult);

        /// <summary>
        /// Initiates a change to the deployment. This works against through the slot name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=config")]
        IAsyncResult BeginChangeConfigurationBySlot(string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input, AsyncCallback callback, object state);

        void EndChangeConfigurationBySlot(IAsyncResult asyncResult);

        #endregion

        #region Resume Deployment Update

        /// <summary>
        /// Resume a suspended deployment update
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=resume")]
        IAsyncResult BeginResumeDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

        void EndResumeDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

        /// <summary>
        ///  Resume a suspended deployment update by slot
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=resume")]
        IAsyncResult BeginResumeDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

        void EndResumeDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

        #endregion

        #region Suspend Deployment Update

        /// <summary>
        /// Suspend a deployment update
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=suspend")]
        IAsyncResult BeginSuspendDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, AsyncCallback callback, object state);

        void EndSuspendDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

        /// <summary>
        ///  Suspend a deployment update by slot
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=suspend")]
        IAsyncResult BeginSuspendDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, AsyncCallback callback, object state);

        void EndSuspendDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

        #endregion

        #region Update Deployment Status

        /// <summary>
        /// Initiates a change to the deployment. This works against through the deployment name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=status")]
        IAsyncResult BeginUpdateDeploymentStatus(string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input, AsyncCallback callback, object state);

        void EndUpdateDeploymentStatus(IAsyncResult asyncResult);

        /// <summary>
        /// Initiates a change to the deployment. This works against through the slot name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=status")]
        IAsyncResult BeginUpdateDeploymentStatusBySlot(string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input, AsyncCallback callback, object state);

        void EndUpdateDeploymentStatusBySlot(IAsyncResult asyncResult);

        #endregion

        #region Upgrade Deployment

        /// <summary>
        /// Initiates an deployment upgrade.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=upgrade")]
        IAsyncResult BeginUpgradeDeployment(string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input, AsyncCallback callback, object state);

        void EndUpgradeDeployment(IAsyncResult asyncResult);

        /// <summary>
        /// Initiates an deployment upgrade through the slot name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=upgrade")]
        IAsyncResult BeginUpgradeDeploymentBySlot(string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input, AsyncCallback callback, object state);

        void EndUpgradeDeploymentBySlot(IAsyncResult asyncResult);

        #endregion

        #region Walk Upgrade Domain

        /// <summary>
        /// Initiates an deployment upgrade.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=walkupgradedomain")]
        IAsyncResult BeginWalkUpgradeDomain(string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input, AsyncCallback callback, object state);

        void EndWalkUpgradeDomain(IAsyncResult asyncResult);

        /// <summary>
        /// Initiates an deployment upgrade through the slot name.
        /// This is an asynchronous operation
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=walkupgradedomain")]
        IAsyncResult BeginWalkUpgradeDomainBySlot(string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input, AsyncCallback callback, object state);

        void EndWalkUpgradeDomainBySlot(IAsyncResult asyncResult);

        #endregion

        #region Reboot Deployment Role Instance

        /// <summary>
        /// Reboots a role instance in a deployment by name
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{roleinstancename}?comp=reboot")]
        IAsyncResult BeginRebootDeploymentRoleInstance(string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, AsyncCallback callback, object state);

        void EndRebootDeploymentRoleInstance(IAsyncResult asyncResult);

        #endregion

        #region Rollback Deployment

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/?comp=rollback")]
        IAsyncResult BeginRollbackDeploymentUpdateOrUpgrade(string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input, AsyncCallback callback, object state);

        void EndRollbackDeploymentUpdateOrUpgrade(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/?comp=rollback")]
        IAsyncResult BeginRollbackDeploymentUpdateOrUpgradeBySlot(string subscriptionId, string serviceName, string deploymentSlot, RollbackUpdateOrUpgradeInput input, AsyncCallback callback, object state);

        void EndRollbackDeploymentUpdateOrUpgradeBySlot(IAsyncResult asyncResult);

        #endregion

        #region Reimage Deployment Role Instance

        /// <summary>
        /// Reimages a role instance in a deployment by name
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{roleinstancename}?comp=reimage")]
        IAsyncResult BeginReimageDeploymentRoleInstance(string subscriptionId, string serviceName, string deploymentName, string roleInstanceName, AsyncCallback callback, object state);

        void EndReimageDeploymentRoleInstance(IAsyncResult asyncResult);

        #endregion

        #region Reboot Deployment Role Instance By Slot

        /// <summary>
        /// Reboots a role instance in a deployment by slot 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/roleinstances/{roleinstancename}?comp=reboot")]
        IAsyncResult BeginRebootDeploymentRoleInstanceBySlot(string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName, AsyncCallback callback, object state);

        void EndRebootDeploymentRoleInstanceBySlot(IAsyncResult asyncResult);

        #endregion

        #region Reimage Deployment Role Instance By Slot

        /// <summary>
        /// Reimages a role instance in a deployment by slot
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/roleinstances/{roleinstancename}?comp=reimage")]
        IAsyncResult BeginReimageDeploymentRoleInstanceBySlot(string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName, AsyncCallback callback, object state);

        void EndReimageDeploymentRoleInstanceBySlot(IAsyncResult asyncResult);

        #endregion

        #region Get Package
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deployments/{deploymentName}/package?containerUri={containerUri}&overwriteExisting={overwriteExisting}")]
        IAsyncResult BeginGetPackage(string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting, AsyncCallback callback, object state);

        void EndGetPackage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{deploymentSlot}/package?containerUri={containerUri}&overwriteExisting={overwriteExisting}")]
        IAsyncResult BeginGetPackageBySlot(string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting, AsyncCallback callback, object state);

        void EndGetPackageBySlot(IAsyncResult asyncResult);

        #endregion
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void SwapDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, SwapDeploymentInput input)
        {
            proxy.EndSwapDeployment(proxy.BeginSwapDeployment(subscriptionId, serviceName, input, null, null));
        }

        public static void CreateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, Deployment deployment)
        {
            proxy.EndCreateDeployment(proxy.BeginCreateDeployment(subscriptionId, serviceName, deployment, null, null));
        }

        public static void CreateOrUpdateDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, CreateDeploymentInput input)
        {
            proxy.EndCreateOrUpdateDeployment(proxy.BeginCreateOrUpdateDeployment(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void DeleteDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndDeleteDeployment(proxy.BeginDeleteDeployment(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void DeleteDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndDeleteDeploymentBySlot(proxy.BeginDeleteDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static Deployment GetDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            return proxy.EndGetDeployment(proxy.BeginGetDeployment(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static Deployment GetDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            return proxy.EndGetDeploymentBySlot(proxy.BeginGetDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void SuspendDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndSuspendDeploymentUpdateOrUpgrade(proxy.BeginSuspendDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void SuspendDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndSuspendDeploymentUpdateOrUpgradeBySlot(proxy.BeginSuspendDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void ResumeDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName)
        {
            proxy.EndResumeDeploymentUpdateOrUpgrade(proxy.BeginResumeDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, null, null));
        }

        public static void ResumeDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot)
        {
            proxy.EndResumeDeploymentUpdateOrUpgradeBySlot(proxy.BeginResumeDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, deploymentSlot, null, null));
        }

        public static void UpdateDeploymentStatus(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpdateDeploymentStatusInput input)
        {
            proxy.EndUpdateDeploymentStatus(proxy.BeginUpdateDeploymentStatus(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void UpdateDeploymentStatusBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpdateDeploymentStatusInput input)
        {
            proxy.EndUpdateDeploymentStatusBySlot(proxy.BeginUpdateDeploymentStatusBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void ChangeConfiguration(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, ChangeConfigurationInput input)
        {
            proxy.EndChangeConfiguration(proxy.BeginChangeConfiguration(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void ChangeConfigurationBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, ChangeConfigurationInput input)
        {
            proxy.EndChangeConfigurationBySlot(proxy.BeginChangeConfigurationBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void UpgradeDeployment(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, UpgradeDeploymentInput input)
        {
            proxy.EndUpgradeDeployment(proxy.BeginUpgradeDeployment(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void UpgradeDeploymentBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, UpgradeDeploymentInput input)
        {
            proxy.EndUpgradeDeploymentBySlot(proxy.BeginUpgradeDeploymentBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void WalkUpgradeDomain(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, WalkUpgradeDomainInput input)
        {
            proxy.EndWalkUpgradeDomain(proxy.BeginWalkUpgradeDomain(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void WalkUpgradeDomainBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, WalkUpgradeDomainInput input)
        {
            proxy.EndWalkUpgradeDomainBySlot(proxy.BeginWalkUpgradeDomainBySlot(subscriptionId, serviceName, deploymentSlot, input, null, null));
        }

        public static void RollbackDeploymentUpdateOrUpgrade(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, RollbackUpdateOrUpgradeInput input)
        {
            proxy.EndRollbackDeploymentUpdateOrUpgrade(proxy.BeginRollbackDeploymentUpdateOrUpgrade(subscriptionId, serviceName, deploymentName, input, null, null));
        }

        public static void RollbackDeploymentUpdateOrUpgradeBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string slotName, RollbackUpdateOrUpgradeInput input)
        {
            proxy.EndRollbackDeploymentUpdateOrUpgradeBySlot(proxy.BeginRollbackDeploymentUpdateOrUpgradeBySlot(subscriptionId, serviceName, slotName, input, null, null));
        }

        public static void RebootDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
        {
            proxy.EndRebootDeploymentRoleInstance(proxy.BeginRebootDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
        }

        public static void ReimageDeploymentRoleInstance(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string roleInstanceName)
        {
            proxy.EndReimageDeploymentRoleInstance(proxy.BeginReimageDeploymentRoleInstance(subscriptionId, serviceName, deploymentName, roleInstanceName, null, null));
        }

        public static void RebootDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
        {
            proxy.EndRebootDeploymentRoleInstanceBySlot(proxy.BeginRebootDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
        }

        public static void ReimageDeploymentRoleInstanceBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string roleInstanceName)
        {
            proxy.EndReimageDeploymentRoleInstanceBySlot(proxy.BeginReimageDeploymentRoleInstanceBySlot(subscriptionId, serviceName, deploymentSlot, roleInstanceName, null, null));
        }

        public static void GetPackage(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentName, string containerUri, bool overwriteExisting)
        {
            proxy.EndGetPackage(proxy.BeginGetPackage(subscriptionId, serviceName, deploymentName, containerUri, overwriteExisting, null, null));
        }

        public static void GetPackageBySlot(this IServiceManagement proxy, string subscriptionId, string serviceName, string deploymentSlot, string containerUri, bool overwriteExisting)
        {
            proxy.EndGetPackageBySlot(proxy.BeginGetPackageBySlot(subscriptionId, serviceName, deploymentSlot, containerUri, overwriteExisting, null, null));
        }
    }
}