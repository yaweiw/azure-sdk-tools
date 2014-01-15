﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Storage.Blob.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Model.Contract;
    using Model.ResourceModel;

    /// <summary>
    /// List azure storage container
    /// </summary>
    [Cmdlet(VerbsCommon.Get, StorageNouns.Container, DefaultParameterSetName = NameParameterSet),
        OutputType(typeof(AzureStorageContainer))]
    public class GetAzureStorageContainerCommand : StorageCloudBlobCmdletBase
    {
        /// <summary>
        /// Default parameter set name
        /// </summary>
        private const string NameParameterSet = "ContainerName";

        /// <summary>
        /// Prefix parameter set name
        /// </summary>
        private const string PrefixParameterSet = "ContainerPrefix";

        [Alias("N", "Container")]
        [Parameter(Position = 0, HelpMessage = "Container Name",
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            ParameterSetName = NameParameterSet)]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Container Prefix",
            ParameterSetName = PrefixParameterSet, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Prefix { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "The max count of the containers that can return.")]
        public int? MaxCount
        {
            get { return InternalMaxCount; }
            set
            {
                if (value.Value <= 0)
                {
                    InternalMaxCount = int.MaxValue;
                }
                else
                {
                    InternalMaxCount = value.Value;
                }
            }
        }

        private int InternalMaxCount = int.MaxValue;

        [Parameter(Mandatory = false, HelpMessage = "Continuation Token.")]
        public BlobContinuationToken ContinuationToken { get; set; }

        /// <summary>
        /// Initializes a new instance of the GetAzureStorageContainerCommand class.
        /// </summary>
        public GetAzureStorageContainerCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureStorageContainerCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public GetAzureStorageContainerCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// List containers by container name pattern.
        /// </summary>
        /// <param name="name">Container name pattern</param>
        /// <returns>An enumerable collection of cloudblob container</returns>
        internal IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> ListContainersByName(string name)
        {
            string prefix = string.Empty;
            BlobRequestOptions requestOptions = RequestOptions;
            AccessCondition accessCondition = null;

            if (String.IsNullOrEmpty(name) || WildcardPattern.ContainsWildcardCharacters(name))
            {
                prefix = NameUtil.GetNonWildcardPrefix(name);
                WildcardOptions options = WildcardOptions.IgnoreCase | WildcardOptions.Compiled;
                WildcardPattern wildcard = null;

                if (!string.IsNullOrEmpty(name))
                {
                    wildcard = new WildcardPattern(name, options);
                }

                Func<CloudBlobContainer, bool> containerFilter = (container) => null == wildcard || wildcard.IsMatch(container.Name);
                IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> containerList = ListContainersByPrefix(prefix, containerFilter);

                foreach (var containerInfo in containerList)
                {
                    yield return containerInfo;
                }
            }
            else
            {
                if (!NameUtil.IsValidContainerName(name))
                {
                    throw new ArgumentException(String.Format(Resources.InvalidContainerName, name));
                }

                CloudBlobContainer container = Channel.GetContainerReference(name);

                if (Channel.DoesContainerExist(container, requestOptions, OperationContext))
                {
                    //fetch container attributes
                    Channel.FetchContainerAttributes(container, accessCondition, requestOptions, OperationContext);
                    yield return new Tuple<CloudBlobContainer, BlobContinuationToken>(container, null);
                }
                else
                {
                    throw new ResourceNotFoundException(String.Format(Resources.ContainerNotFound, name));
                }
            }
        }

        /// <summary>
        /// List containers by container name prefix
        /// </summary>
        /// <param name="prefix">Container name prefix</param>
        /// <returns>An enumerable collection of cloudblobcontainer</returns>
        internal IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> ListContainersByPrefix(string prefix, Func<CloudBlobContainer, bool> containerFilter = null)
        {
            ContainerListingDetails details = ContainerListingDetails.Metadata;
            BlobRequestOptions requestOptions = RequestOptions;

            if (!string.IsNullOrEmpty(prefix) && !NameUtil.IsValidContainerPrefix(prefix))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, prefix));
            }

            int listCount = InternalMaxCount;
            int MaxListCount = 5000;
            int requestCount = MaxListCount;
            int realListCount = 0;
            BlobContinuationToken continuationToken = ContinuationToken;

            do
            {
                requestCount = Math.Min(listCount, MaxListCount);
                realListCount = 0;

                ContainerResultSegment containerResult = Channel.ListContainersSegmented(prefix, details, requestCount, continuationToken, requestOptions, OperationContext);

                foreach (CloudBlobContainer container in containerResult.Results)
                {
                    if (containerFilter == null || containerFilter(container))
                    {
                        yield return new Tuple<CloudBlobContainer, BlobContinuationToken>(container, containerResult.ContinuationToken);
                        realListCount++;
                    }
                }

                if (InternalMaxCount != int.MaxValue)
                {
                    listCount -= realListCount;
                }

                continuationToken = containerResult.ContinuationToken;
            }
            while (listCount > 0 && continuationToken != null);
        }

        /// <summary>
        /// Pack CloudBlobContainer and it's permission to AzureStorageContainer object
        /// </summary>
        /// <param name="containerList">An enumerable collection of CloudBlobContainer</param>
        /// <returns>An enumerable collection of AzureStorageContainer</returns>
        internal IEnumerable<AzureStorageContainer> PackCloudBlobContainerWithAcl(IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> containerList)
        {
            if (null == containerList)
            {
                yield break;
            }

            BlobRequestOptions requestOptions = RequestOptions;
            AccessCondition accessCondition = null;

            foreach (Tuple<CloudBlobContainer, BlobContinuationToken> containerItem in containerList)
            {
                CloudBlobContainer container = containerItem.Item1;
                BlobContainerPermissions permissions = null;

                try
                {
                    permissions = Channel.GetContainerPermissions(container, accessCondition, requestOptions, OperationContext);
                }
                catch (Exception e)
                {
                    //Log the error message and continue the process
                    WriteVerboseWithTimestamp(String.Format(Resources.GetContainerPermissionException, container.Name, e.Message));
                }

                AzureStorageContainer azureContainer = new AzureStorageContainer(container, permissions);
                azureContainer.ContinuationToken = containerItem.Item2;
                yield return azureContainer;
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> containerList = null;

            if (PrefixParameterSet == ParameterSetName)
            {
                containerList = ListContainersByPrefix(Prefix);
            }
            else
            {
                containerList = ListContainersByName(Name);
            }

            IEnumerable<AzureStorageContainer> azureContainers = PackCloudBlobContainerWithAcl(containerList);
            WriteObjectWithStorageContext(azureContainers);
        }
    }
}
