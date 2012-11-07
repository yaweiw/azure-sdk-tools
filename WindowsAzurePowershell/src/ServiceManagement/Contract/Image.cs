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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    /// <summary>
    /// The image-specific part of the service management service. This will be deprecated in favor of new image repository.
    /// </summary>
    public partial interface IServiceManagement
    {
        /// <summary>
        /// Prepare an image for upload.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/machineimages/{imageName}")]
        IAsyncResult BeginPrepareImageUpload(string subscriptionId, string imageName, PrepareImageUploadInput input, AsyncCallback callback, object state);

        void EndPrepareImageUpload(IAsyncResult asyncResult);

        /// <summary>
        /// Get image reference.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/machineimages/{imageName}?comp=reference&expiry={expiry}&permission={permission}")]
        IAsyncResult BeginGetImageReference(string subscriptionID, string imageName, string expiry, string permission, AsyncCallback callback, object state);

        MachineImageReference EndGetImageReference(IAsyncResult asyncResult);

        /// <summary>
        /// Commit the upload of an image.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/machineimages/{imageName}?comp=commitmachineimage")]
        IAsyncResult BeginCommitImageUpload(string subscriptionID, string imageName, AsyncCallback callback, object state);

        void EndCommitImageUpload(IAsyncResult asyncResult);

        /// <summary>
        /// List all images associated with a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/machineimages")]
        IAsyncResult BeginListImages(string subscriptionID, AsyncCallback callback, object state);

        MachineImageList EndListImages(IAsyncResult asyncResult);

        /// <summary>
        /// Get information about an image.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/machineimages/{imageName}")]
        IAsyncResult BeginGetImageProperties(string subscriptionID, string imageName, AsyncCallback callback, object state);

        MachineImage EndGetImageProperties(IAsyncResult asyncResult);

        /// <summary>
        /// Set image properties.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/machineimages/{imagename}?comp=properties")]
        IAsyncResult BeginSetImageProperties(string subscriptionID, string imageName, SetMachineImagePropertiesInput imageProperties, AsyncCallback callback, object state);

        void EndSetImageProperties(IAsyncResult asyncResult);

        /// <summary>
        /// Set parent image.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/machineimages/{imagename}?comp=setparent")]
        IAsyncResult BeginSetParentImage(string subscriptionID, string imageName, SetParentImageInput parentImageInput, AsyncCallback callback, object state);

        void EndSetParentImage(IAsyncResult asyncResult);

        /// <summary>
        /// Delete an image.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/machineimages/{imageName}")]
        IAsyncResult BeginDeleteImage(string subscriptionID, string imageName, AsyncCallback callback, object state);

        void EndDeleteImage(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void PrepareImageUpload(this IServiceManagement proxy, string subscriptionID, string imageName, PrepareImageUploadInput input)
        {
            proxy.EndPrepareImageUpload(proxy.BeginPrepareImageUpload(subscriptionID, imageName, input, null, null));
        }

        public static MachineImageReference GetImageReference(this IServiceManagement proxy, string subscriptionId, string imageName, DateTime expiry, ImageSharedAccessSignaturePermission accessModifier)
        {
            return proxy.EndGetImageReference(proxy.BeginGetImageReference(subscriptionId, imageName, expiry.ToString("o"), accessModifier.ToString().ToLower(), null, null));
        }

        public static void CommitImageUpload(this IServiceManagement proxy, string subscriptionId, string imageName)
        {
            proxy.EndCommitImageUpload(proxy.BeginCommitImageUpload(subscriptionId, imageName, null, null));
        }

        public static MachineImageList ListImages(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListImages(proxy.BeginListImages(subscriptionID, null, null));
        }

        public static MachineImage GetImageProperties(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            return proxy.EndGetImageProperties(proxy.BeginGetImageProperties(subscriptionID, imageName, null, null));
        }

        public static void SetImageProperties(this IServiceManagement proxy, string subscriptionID, string imageName, SetMachineImagePropertiesInput input)
        {
            proxy.EndSetImageProperties(proxy.BeginSetImageProperties(subscriptionID, imageName, input, null, null));
        }

        public static void SetParentImage(this IServiceManagement proxy, string subscriptionID, string imageName, SetParentImageInput input)
        {
            proxy.EndSetParentImage(proxy.BeginSetParentImage(subscriptionID, imageName, input, null, null));
        }

        public static void DeleteImage(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            proxy.EndDeleteImage(proxy.BeginDeleteImage(subscriptionID, imageName, null, null));
        }
    }
}