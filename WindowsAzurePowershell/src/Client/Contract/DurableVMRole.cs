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
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    #region IServiceManagement
    public partial interface IServiceManagement
    {
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles")]
        [ServiceKnownType(typeof(PersistentVMRole))]
        IAsyncResult BeginAddRole(string subscriptionID,
            string serviceName,
            string deploymentName,
            Role role,
            AsyncCallback callback, object state);
        void EndAddRole(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
        [ServiceKnownType(typeof(PersistentVMRole))]
        IAsyncResult BeginGetRole(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            AsyncCallback callback, object state);
        Role EndGetRole(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
        [ServiceKnownType(typeof(PersistentVMRole))]
        IAsyncResult BeginUpdateRole(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            Role role,
            AsyncCallback callback, object state);
        void EndUpdateRole(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}")]
        IAsyncResult BeginDeleteRole(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            AsyncCallback callback, object state);
        void EndDeleteRole(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks")]
        IAsyncResult BeginAddDataDisk(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            DataVirtualHardDisk dataDisk,
            AsyncCallback callback, object state);
        void EndAddDataDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
        IAsyncResult BeginUpdateDataDisk(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            string lun,
            DataVirtualHardDisk dataDisk,
            AsyncCallback callback, object state);
        void EndUpdateDataDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
        IAsyncResult BeginGetDataDisk(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            string lun,
            AsyncCallback callback, object state);
        DataVirtualHardDisk EndGetDataDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/Roles/{roleName}/DataDisks/{lun}")]
        IAsyncResult BeginDeleteDataDisk(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleName,
            string lun,
            AsyncCallback callback, object state);
        void EndDeleteDataDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleInstances/{roleInstanceName}/Operations")]
        [ServiceKnownType(typeof(CaptureRoleOperation))]
        [ServiceKnownType(typeof(ShutdownRoleOperation))]
        [ServiceKnownType(typeof(StartRoleOperation))]
        [ServiceKnownType(typeof(RestartRoleOperation))]        
        IAsyncResult BeginExecuteRoleOperation(string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleInstanceName,
            RoleOperation roleOperation,
            AsyncCallback callback, object state);
        void EndExecuteRoleOperation(IAsyncResult asyncResult);

        //MIXEDMODE::[OperationContract(AsyncPattern = true)]
        //[WebGet(UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/ModelFile?Type={modelFileType}")]
        //IAsyncResult BeginDownloadModelFile(string subscriptionID,
        //    string serviceName,
        //    string deploymentName,
        //    string modelFileType,
        //    AsyncCallback callback, object state);
        //Stream EndDownloadModelFile(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/hostedservices/{serviceName}/deployments/{deploymentName}/roleinstances/{instanceName}/ModelFile?FileType=RDP")]
        IAsyncResult BeginDownloadRDPFile(string subscriptionID,
            string serviceName,
            string deploymentName,
            string instanceName,            
            AsyncCallback callback, object state);
        Stream EndDownloadRDPFile(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void CaptureRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName,
            string targetImageName,
            string targetImageLabel,
            PostCaptureAction postCaptureAction,
            ProvisioningConfigurationSet provisioningConfiguration)
        {
            proxy.EndExecuteRoleOperation(
                proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new CaptureRoleOperation
                {
                    PostCaptureAction = postCaptureAction.ToString(),
                    ProvisioningConfiguration = provisioningConfiguration,
                    TargetImageName = targetImageName,
                    TargetImageLabel = targetImageLabel
                }, null, null));
        }

        public static void ShutdownRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new ShutdownRoleOperation(),
                null,
                null));
        }

        public static void StartRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new StartRoleOperation(),
                null,
                null));
        }

        public static void RestartRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            proxy.EndExecuteRoleOperation(proxy.BeginExecuteRoleOperation(
                subscriptionId,
                serviceName,
                deploymentName,
                roleInstanceName,
                new RestartRoleOperation(),
                null,
                null));
        }

        public static void AddDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            DataVirtualHardDisk dataDisk)
        {
            proxy.EndAddDataDisk(proxy.BeginAddDataDisk(
                subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                dataDisk,
                null,
                null));
        }

        public static void UpdateDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun,
            DataVirtualHardDisk dataDisk)
        {
            proxy.EndUpdateDataDisk(
                proxy.BeginUpdateDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                dataDisk,
                null,
                null));
        }

        public static void DeleteDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun)
        {
            proxy.EndDeleteDataDisk(
                proxy.BeginDeleteDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                null,
                null));
        }

        public static DataVirtualHardDisk GetDataDisk(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            int lun)
        {
            return proxy.EndGetDataDisk(
                proxy.BeginGetDataDisk(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                lun.ToString(),
                null,
                null));
        }

        public static void AddRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            PersistentVMRole role)
        {
            proxy.EndAddRole(
                proxy.BeginAddRole(subscriptionId,
                serviceName,
                deploymentName,
                role,
                null,
                null));
        }

        public static void UpdateRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName,
            PersistentVMRole role)
        {
            proxy.EndUpdateRole(
                proxy.BeginUpdateRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                role,
                null,
                null));
        }

        public static PersistentVMRole GetRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName)
        {
            return (PersistentVMRole)proxy.EndGetRole(
                proxy.BeginGetRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                null,
                null));
        }

        public static void DeleteRole(this IServiceManagement proxy,
            string subscriptionId,
            string serviceName,
            string deploymentName,
            string roleName)
        {
            proxy.EndDeleteRole(
                proxy.BeginDeleteRole(subscriptionId,
                serviceName,
                deploymentName,
                roleName,
                null,
                null));
        }

        //MIXEDMODE
        //public static Stream DownloadModelFile(this IServiceManagement proxy,
        //    string subscriptionID,
        //    string serviceName,
        //    string deploymentName,
        //    string modelFileType)
        //{
        //    return proxy.EndDownloadModelFile(
        //        proxy.BeginDownloadModelFile(
        //        subscriptionID,
        //        serviceName,
        //        deploymentName,
        //        modelFileType,
        //        null,
        //        null));
        //}

        public static Stream DownloadRDPFile(this IServiceManagement proxy,
            string subscriptionID,
            string serviceName,
            string deploymentName,
            string roleInstanceName)
        {
            return proxy.EndDownloadRDPFile(
                proxy.BeginDownloadRDPFile(
                subscriptionID,
                serviceName,
                deploymentName,
                roleInstanceName,
                null,
                null));
        }
    }
    #endregion
}
