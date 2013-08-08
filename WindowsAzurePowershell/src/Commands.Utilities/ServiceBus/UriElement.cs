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

namespace Microsoft.WindowsAzure.Commands.Utilities.ServiceBus
{
    internal static class UriElement
    {
        public const string ServiceBusLatestVersion = "2013-07";

        public static string ConnectionStringUri(string namespaceName)
        {
            return string.Format("{0}/ConnectionDetails/?api-version={1}", namespaceName, ServiceBusLatestVersion);
        }

        public static string ConnectionStringUri(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType)
        {
            return string.Format("{0}/{1}s/{2}/ConnectionDetails/?api-version={3}",
                namespaceName,
                entityType.ToString(),
                entityName,
                ServiceBusLatestVersion);
        }

        public static string GetNamespaceAuthorizationRulesPath(string namespaceName)
        {
            return namespaceName + "/AuthorizationRules/";
        }

        public static string GetAuthorizationRulePath(string namespaceName, string ruleName)
        {
            return string.Format("{0}/AuthorizationRules/{1}", namespaceName, ruleName);
        }
    }
}