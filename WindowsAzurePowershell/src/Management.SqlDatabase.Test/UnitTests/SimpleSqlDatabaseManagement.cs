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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests
{
    using System;
    using System.Xml;
    using Management.Test.Tests.Utilities;
    using Services;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Simple implementation of the <see cref="ISqlDatabaseManagement"/> interface that can be
    /// used for mocking basic interactions without involving Azure directly.
    /// </summary>
    public class SimpleSqlDatabaseManagement : ISqlDatabaseManagement
    {
        /// <summary>
        /// Gets or sets a value indicating whether the thunk wrappers will
        /// throw an exception if the thunk is not implemented.  This is useful
        /// when debugging a test.
        /// </summary>
        public bool ThrowsIfNotImplemented { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleSqlDatabaseManagement"/> class.
        /// </summary>
        public SimpleSqlDatabaseManagement()
        {
            ThrowsIfNotImplemented = true;
        }

        #region Implementation Thunks

        #region GetServers

        public Func<SimpleServiceManagementAsyncResult, SqlDatabaseServerList> GetServersThunk { get; set; }
        public IAsyncResult BeginGetServers(string subscriptionId, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values[
                ] = subscriptionId;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public SqlDatabaseServerList EndGetServers(IAsyncResult asyncResult)
        {
            if (GetServersThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetServersThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetServersThunk is not implemented!");
            }

            return default(SqlDatabaseServerList);
        }

        #endregion

        #region NewServer

        public Func<SimpleServiceManagementAsyncResult, XmlElement> NewServerThunk { get; set; }
        public IAsyncResult BeginNewServer(string subscriptionId, NewSqlDatabaseServerInput input, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["input"] = input;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public XmlElement EndNewServer(IAsyncResult asyncResult)
        {
            if (NewServerThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return NewServerThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("NewServerThunk is not implemented!");
            }

            return default(XmlElement);
        }

        #endregion

        #region RemoveServer

        public Action<SimpleServiceManagementAsyncResult> RemoveServerThunk { get; set; }
        public IAsyncResult BeginRemoveServer(string subscriptionId, string serverName, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndRemoveServer(IAsyncResult asyncResult)
        {
            if (RemoveServerThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                RemoveServerThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("RemoveServerThunk is not implemented!");
            }
        }

        #endregion

        #region SetPassword

        public Action<SimpleServiceManagementAsyncResult> SetPasswordThunk { get; set; }
        public IAsyncResult BeginSetPassword(string subscriptionId, string serverName, XmlElement password, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["password"] = password;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndSetPassword(IAsyncResult asyncResult)
        {
            if (SetPasswordThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                SetPasswordThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("SetPasswordThunk is not implemented!");
            }
        }

        #endregion

        #region GetServerFirewallRules

        public Func<SimpleServiceManagementAsyncResult, SqlDatabaseFirewallRulesList> GetServerFirewallRulesThunk { get; set; }
        public IAsyncResult BeginGetServerFirewallRules(string subscriptionId, string serverName, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public SqlDatabaseFirewallRulesList EndGetServerFirewallRules(IAsyncResult asyncResult)
        {
            if (GetServerFirewallRulesThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetServerFirewallRulesThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetServerFirewallRulesThunk is not implemented!");
            }

            return default(SqlDatabaseFirewallRulesList);
        }

        #endregion

        #region NewServerFirewallRule

        public Action<SimpleServiceManagementAsyncResult> NewServerFirewallRuleThunk { get; set; }
        public IAsyncResult BeginNewServerFirewallRule(string subscriptionId, string serverName, SqlDatabaseFirewallRuleInput input, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["input"] = input;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndNewServerFirewallRule(IAsyncResult asyncResult)
        {
            if (NewServerFirewallRuleThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                NewServerFirewallRuleThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("NewServerFirewallRuleThunk is not implemented!");
            }
        }

        #endregion

        #region UpdateServerFirewallRule

        public Action<SimpleServiceManagementAsyncResult> UpdateServerFirewallRuleThunk { get; set; }
        public IAsyncResult BeginUpdateServerFirewallRule(string subscriptionId, string serverName, string ruleName, SqlDatabaseFirewallRuleInput input, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["ruleName"] = ruleName;
            result.Values["input"] = input;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndUpdateServerFirewallRule(IAsyncResult asyncResult)
        {
            if (UpdateServerFirewallRuleThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                UpdateServerFirewallRuleThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("UpdateServerFirewallRuleThunk is not implemented!");
            }
        }

        #endregion

        #region RemoveServerFirewallRule

        public Action<SimpleServiceManagementAsyncResult> RemoveServerFirewallRuleThunk { get; set; }
        public IAsyncResult BeginRemoveServerFirewallRule(string subscriptionId, string serverName, string ruleName, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serverName"] = serverName;
            result.Values["ruleName"] = ruleName;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndRemoveServerFirewallRule(IAsyncResult asyncResult)
        {
            if (RemoveServerFirewallRuleThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                RemoveServerFirewallRuleThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("RemoveServerFirewallRuleThunk is not implemented!");
            }
        }

        #endregion
     
        #endregion
    }
}

