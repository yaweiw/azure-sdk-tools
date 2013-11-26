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

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters
{
    using System;
    using System.Linq;
    using BaseInterfaces;
    using Commands.CommandImplementations;
    using Extensions;
    using Hadoop.Client;
    using WindowsAzure.Commands.Utilities.Common;

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

        public static IJobSubmissionClientCredential GetJobSubmissionClientCredentials(this IAzureHDInsightJobCommandCredentialsBase command, WindowsAzureSubscription currentSubscription, string cluster)
        {
            IJobSubmissionClientCredential clientCredential = null;
            if (command.Credential != null)
            {
                clientCredential = new BasicAuthCredential
                {
                    Server = GatewayUriResolver.GetGatewayUri(cluster),
                    UserName = command.Credential.UserName,
                    Password = command.Credential.GetCleartextPassword()
                };
            }
            else if (currentSubscription.IsNotNull())
            {
                var subscriptionCredentials = GetSubscriptionCredentials(command, currentSubscription);
                var asCertificateCredentials = subscriptionCredentials as HDInsightCertificateCredential;
                var asTokenCredentials = subscriptionCredentials as HDInsightAccessTokenCredential;
                if (asCertificateCredentials.IsNotNull())
                {
                    clientCredential = new JobSubmissionCertificateCredential(asCertificateCredentials, cluster);
                }
                else if (asTokenCredentials.IsNotNull())
                {
                    clientCredential = new JobSubmissionAccessTokenCredential(asTokenCredentials, cluster);
                }
            }

            return clientCredential;
        }

        private static Guid ResolveSubscriptionId(string subscription)
        {
            Guid subscriptionId;
            Guid.TryParse(subscription, out subscriptionId);
            return subscriptionId;
        }


        internal static string GetClusterName(string clusterNameOrUri)
        {
            Uri clusterUri;
            if (Uri.TryCreate(clusterNameOrUri, UriKind.Absolute, out clusterUri))
            {
                return clusterUri.DnsSafeHost.Split('.').First();
            }

            return clusterNameOrUri.Split('.').First();
        }
    }
}
