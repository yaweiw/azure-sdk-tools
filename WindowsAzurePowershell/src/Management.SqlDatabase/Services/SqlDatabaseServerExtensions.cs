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
    using System.IO;
    using System.Xml;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    public static partial class SqlDatabaseManagementExtensionMethods
    {
        public static SqlDatabaseServerList GetServers(this ISqlDatabaseManagement proxy, string subscriptionId)
        {
            return proxy.EndGetServers(proxy.BeginGetServers(subscriptionId, null, null));
        }

        public static XmlElement NewServer(this ISqlDatabaseManagement proxy, string subscriptionId, string administratorLogin, string administratorLoginPassword, string location)
        {
            var input = new NewSqlDatabaseServerInput()
            {
                AdministratorLogin = administratorLogin,
                AdministratorLoginPassword = administratorLoginPassword,
                Location = location
            };

            var inputproxy = proxy.BeginNewServer(subscriptionId, input, null, null);
            var result = proxy.EndNewServer(inputproxy);
            return result;
        }

        public static void RemoveServer(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName)
        {
            proxy.EndRemoveServer(proxy.BeginRemoveServer(subscriptionId, serverName, null, null));
        }

        public static void SetPassword(this ISqlDatabaseManagement proxy, string subscriptionId, string serverName, string password)
        {
            // create an xml element for the request body
            var xml = string.Empty;

            using (var tx = new StringWriter())
            {
                var tw = new XmlTextWriter(tx);
                tw.WriteStartDocument();
                tw.WriteStartElement("AdministratorLoginPassword", Constants.SqlDatabaseManagementNamespace);
                tw.WriteString(password);
                tw.WriteEndElement();

                xml = tx.ToString();
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var el = (XmlElement)doc.FirstChild.NextSibling;

            proxy.EndSetPassword(proxy.BeginSetPassword(subscriptionId, serverName, el, null, null));
        }
    }
}