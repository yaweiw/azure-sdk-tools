// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters
{
    using System;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.Extensions;

    internal static class AzureHDInsightCommandExtensions
    {
        public static IHDInsightSubscriptionCredentials GetSubscriptionCredentials(this IAzureHDInsightCommonCommandBase command, WindowsAzureSubscription currentSubscription)
        {
            if (currentSubscription.Certificate.IsNotNull())
            {
                return GetSubscriptionCertificateCredentials(command, currentSubscription);
            }
            else if (currentSubscription.ActiveDirectoryUserId.IsNotNull())
            {
                return GetAccessTokenCredentials(command, currentSubscription);
            }

            throw new NotSupportedException();
        }

        public static IHDInsightSubscriptionCredentials GetSubscriptionCertificateCredentials(this IAzureHDInsightCommonCommandBase command, WindowsAzureSubscription currentSubscription)
        {
            return new HDInsightCertificateCredential
            {
                SubscriptionId = ResolveSubscriptionId(currentSubscription.SubscriptionId),
                Certificate = currentSubscription.Certificate,
                Endpoint = currentSubscription.ServiceEndpoint,
            };
        }

        public static IHDInsightSubscriptionCredentials GetAccessTokenCredentials(this IAzureHDInsightCommonCommandBase command, WindowsAzureSubscription currentSubscription)
        {
            var accessToken = currentSubscription.TokenProvider.GetCachedToken(currentSubscription,
                                                                       currentSubscription.ActiveDirectoryUserId);
            return new HDInsightAccessTokenCredential()
            {
                SubscriptionId = ResolveSubscriptionId(currentSubscription.SubscriptionId),
                AccessToken = accessToken.AccessToken
            };
        }

        private static Guid ResolveSubscriptionId(string subscription)
        {
            Guid subscriptionId;
            Guid.TryParse(subscription, out subscriptionId);
            return subscriptionId;
        }
    }
}
