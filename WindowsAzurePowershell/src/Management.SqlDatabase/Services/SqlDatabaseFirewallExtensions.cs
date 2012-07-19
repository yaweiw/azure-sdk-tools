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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Services
{
    public static partial class SqlDatabaseManagementExtensionMethods
    {
        public static SqlDatabaseFirewallRulesList GetServerFirewallRules(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName)
        {
            return proxy.EndGetServerFirewallRules(proxy.BeginGetServerFirewallRules(subscriptionId, serverName, null, null));
        }

        public static void NewServerFirewallRule(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName, string ruleName, string startIpAddress, string endIpAddress)
        {
            var input = new SqlDatabaseFirewallRuleInput
            {
                Name = ruleName,
                StartIPAddress = startIpAddress,
                EndIPAddress = endIpAddress
            };

            proxy.EndNewServerFirewallRule(proxy.BeginNewServerFirewallRule(subscriptionId, serverName, input, null, null));
        }

        public static void UpdateServerFirewallRule(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName, string ruleName, string startIpAddress, string endIpAddress)
        {
            var input = new SqlDatabaseFirewallRuleInput
            {
                Name = ruleName,
                StartIPAddress = startIpAddress,
                EndIPAddress = endIpAddress
            };

            proxy.EndUpdateServerFirewallRule(proxy.BeginUpdateServerFirewallRule(subscriptionId, serverName, ruleName, input, null, null));
        }

        public static void RemoveServerFirewallRule(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName, string ruleName)
        {
            proxy.EndRemoveServerFirewallRule(proxy.BeginRemoveServerFirewallRule(subscriptionId, serverName, ruleName, null, null));
        }
    }
}
