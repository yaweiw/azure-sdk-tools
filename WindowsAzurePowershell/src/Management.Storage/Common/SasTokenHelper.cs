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

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Management.Storage.Model.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;

    internal class SasTokenHelper
    {
        /// <summary>
        /// Validate the container access policy
        /// </summary>
        /// <param name="policy">SharedAccessBlobPolicy object</param>
        /// <param name="policyIdentifier">The policy identifier which need to be checked.</param>
        public static void ValidateContainerAccessPolicy(IStorageBlobManagement channel, string containerName, 
            SharedAccessBlobPolicy policy, string policyIdentifier)
        {
            if (string.IsNullOrEmpty(policyIdentifier)) return;
            CloudBlobContainer container = channel.GetContainerReference(containerName);
            AccessCondition accessCondition = null;
            BlobRequestOptions options = null;
            OperationContext context = null;
            BlobContainerPermissions permission = channel.GetContainerPermissions(container, accessCondition, options, context);

            ValidateExistingPolicy<SharedAccessBlobPolicy>(permission.SharedAccessPolicies, policyIdentifier);
        }

        /// <summary>
        /// Validate the queue access policy
        /// </summary>
        /// <param name="policy">SharedAccessBlobPolicy object</param>
        /// <param name="policyIdentifier">The policy identifier which need to be checked.</param>
        public static void ValidateQueueAccessPolicy(IStorageQueueManagement channel, string queueName,
            SharedAccessQueuePolicy policy, string policyIdentifier)
        {
            if (string.IsNullOrEmpty(policyIdentifier)) return;
            CloudQueue queue = channel.GetQueueReference(queueName);
            QueueRequestOptions options = null;
            OperationContext context = null;
            QueuePermissions permission = channel.GetPermissions(queue, options, context);

            ValidateExistingPolicy<SharedAccessQueuePolicy>(permission.SharedAccessPolicies, policyIdentifier);
        }

        /// <summary>
        /// Validate the table access policy
        /// </summary>
        /// <param name="policy">SharedAccessBlobPolicy object</param>
        /// <param name="policyIdentifier">The policy identifier which need to be checked.</param>
        internal static void ValidateTableAccessPolicy(IStorageTableManagement channel, string tableName, SharedAccessTablePolicy policy, string policyIdentifier)
        {
            if (string.IsNullOrEmpty(policyIdentifier)) return;
            CloudTable table = channel.GetTableReference(tableName);
            TableRequestOptions options = null;
            OperationContext context = null;
            TablePermissions permission = channel.GetTablePermissions(table, options, context);

            ValidateExistingPolicy<SharedAccessTablePolicy>(permission.SharedAccessPolicies, policyIdentifier);
        }

        /// <summary>
        /// Valiate access policy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policies">Access policy</param>
        /// <param name="policyIdentifier">policyIdentifier</param>
        internal static void ValidateExistingPolicy<T>(IDictionary<string, T> policies, string policyIdentifier)
        {
            policyIdentifier = policyIdentifier.ToLower();//policy name should case-insensitive in url.
            foreach (KeyValuePair<string, T> pair in policies)
            {
                if (pair.Key.ToLower() == policyIdentifier)
                {
                    return;
                }
            }

            throw new ArgumentException(string.Format(Resources.InvalidAccessPolicy, policyIdentifier));
        }

        public static void SetupAccessPolicyLifeTime(DateTime? startTime, DateTime? expiryTime, out DateTimeOffset? SharedAccessStartTime, out DateTimeOffset? SharedAccessExpiryTime)
        {
            SharedAccessStartTime = null;
            SharedAccessExpiryTime = null;
            //Set up start/expiry time
            if (startTime != null)
            {
                SharedAccessStartTime = startTime.Value.ToUniversalTime();
            }

            if (expiryTime != null)
            {
                SharedAccessExpiryTime = expiryTime.Value.ToUniversalTime();
            }
            else
            {
                double defaultLifeTime = 1.0; //Hours
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(defaultLifeTime).ToUniversalTime();
            }

            if (SharedAccessStartTime != null && SharedAccessExpiryTime <= SharedAccessStartTime)
            {
                throw new ArgumentException(String.Format(Resources.ExpiryTimeGreatThanStartTime,
                    SharedAccessExpiryTime.ToString(), SharedAccessStartTime.ToString()));
            }
        }
    }
}
