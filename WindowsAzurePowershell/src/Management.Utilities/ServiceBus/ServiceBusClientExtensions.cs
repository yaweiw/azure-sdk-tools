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

namespace Microsoft.WindowsAzure.Management.Utilities.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Management;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceBus.Notifications;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Microsoft.WindowsAzure.Management.Utilities.ServiceBus.Contract;
    using Microsoft.WindowsAzure.Management.Utilities.ServiceBus.ResourceModel;
    using Microsoft.WindowsAzure.ServiceManagement;
    using System.Linq;
    using System.Threading.Tasks;
    using ExtendedAuthorizationRule = Microsoft.WindowsAzure.Management.Utilities.ServiceBus.ResourceModel.AuthorizationRule;
    using AuthorizationRule = Microsoft.ServiceBus.Messaging.AuthorizationRule;
    using System.Diagnostics;

    public class ServiceBusClientExtensions
    {
        private string subscriptionId;

        public SubscriptionData Subscription { get; set; }

        public Action<string> Logger { get; set; }

        public IServiceBusManagement ServiceBusManagementChannel { get; internal set; }

        public const string ACSConnectionStringKeyName = "ACSOwnerKey";

        private HttpClient CreateServiceBusHttpClient()
        {
            WebRequestHandler requestHandler = new WebRequestHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };
            requestHandler.ClientCertificates.Add(Subscription.Certificate);
            StringBuilder endpoint = new StringBuilder(General.EnsureTrailingSlash(Subscription.ServiceEndpoint));
            endpoint.Append(subscriptionId);
            endpoint.Append("/services/servicebus/namespaces/");
            HttpClient client = HttpClientHelper.CreateClient(endpoint.ToString(), handler: requestHandler);
            client.DefaultRequestHeaders.Add(ApiConstants.VersionHeaderName, "2012-03-01");

            return client;
        }

        private List<T> GetAuthorizationRuleCore<T>(string namespaceName) where T : class
        {
            List<T> rules = null;

            using (HttpClient client = CreateServiceBusHttpClient())
            {
                rules = client.GetJson<List<T>>(
                    UriElement.GetNamespaceAuthorizationRulesPath(namespaceName),
                    Logger);
            }

            return rules;
        }

        private NamespaceManager CreateNamespaceManager(string namespaceName)
        {
            return NamespaceManager.CreateFromConnectionString(GetConnectionString(
                namespaceName,
                ACSConnectionStringKeyName));
        }

        private List<ExtendedAuthorizationRule> GetAuthorizationRuleCore(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType,
            Predicate<AuthorizationRule> match)
        {
            NamespaceManager namespaceManager = CreateNamespaceManager(namespaceName);
            List<AuthorizationRule> rules = null;

            switch (entityType)
            {
                case ServiceBusEntityType.Queue:
                    rules = namespaceManager.GetQueue(entityName).Authorization.GetRules(match);
                    break;

                case ServiceBusEntityType.Topic:
                    rules = namespaceManager.GetTopic(entityName).Authorization.GetRules(match);
                    break;

                case ServiceBusEntityType.Relay:
                    rules = namespaceManager.GetRelayAsync(entityName).Result.Authorization.GetRules(match);
                    break;

                case ServiceBusEntityType.NotificationHub:
                    rules = namespaceManager.GetNotificationHub(entityName).Authorization.GetRules(match);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return rules.Select(r => CreateExtendedExtendedAuthorizationRule(
                r,
                namespaceName,
                entityName,
                entityType)).ToList();
        }

        private ExtendedAuthorizationRule CreateExtendedExtendedAuthorizationRule(
            AuthorizationRule rule,
            string namespaceName)
        {
            return new ExtendedAuthorizationRule()
            {
                Rule = rule,
                Name = rule.KeyName,
                Permission = rule.Rights.ToList(),
                ConnectionString = GetConnectionString(namespaceName, rule.KeyName)
            };
        }

        private ExtendedAuthorizationRule CreateExtendedExtendedAuthorizationRule(
            AuthorizationRule rule,
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType)
        {
            return new ExtendedAuthorizationRule()
            {
                Rule = rule,
                Name = rule.KeyName,
                Permission = rule.Rights.ToList(),
                ConnectionString = GetConnectionString(namespaceName, entityName, entityType, rule.KeyName),
                EntityName = entityName,
                EntityType = entityType
            };
        }

        /// <summary>
        /// Parameterless constructs for mocking framework.
        /// </summary>
        public ServiceBusClientExtensions()
        {

        }

        /// <summary>
        /// Creates new instance from ServiceBusManager
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="logger">The logger action</param>
        public ServiceBusClientExtensions(SubscriptionData subscription, Action<string> logger = null)
        {
            subscriptionId = subscription.SubscriptionId;
            Subscription = subscription;
            Logger = logger;
            ServiceBusManagementChannel = ChannelHelper.CreateServiceManagementChannel<IServiceBusManagement>(
                ConfigurationConstants.WebHttpBinding(),
                new Uri(subscription.ServiceEndpoint),
                subscription.Certificate,
                new HttpRestMessageInspector(logger));
        }

        /// <summary>
        /// Gets the connection string with the given name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="keyName">The connection string key name</param>
        /// <returns>The connection string value</returns>
        public virtual string GetConnectionString(string namespaceName, string keyName)
        {
            List<ConnectionDetail> connectionStrings = GetConnectionString(namespaceName);
            ConnectionDetail connectionString = connectionStrings.Find(c => c.KeyName.Equals(
                keyName,
                StringComparison.OrdinalIgnoreCase));

            return connectionString.ConnectionString;
        }

        /// <summary>
        /// Gets the connection string with the given name for the entity.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="keyName">The connection string key name</param>
        /// <returns>The connection string value</returns>
        public virtual string GetConnectionString(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType,
            string keyName)
        {
            List<ConnectionDetail> connectionStrings = GetConnectionString(namespaceName, entityName, entityType);
            ConnectionDetail connectionString = connectionStrings.Find(c => c.KeyName.Equals(
                keyName,
                StringComparison.OrdinalIgnoreCase));

            return connectionString.ConnectionString;
        }

        /// <summary>
        /// Gets available connection strings for the specified entity.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="entityName">The entity name</param>
        /// <param name="entityType">The entity type</param>
        /// <returns>List of all available connection strings</returns>
        public virtual List<ConnectionDetail> GetConnectionString(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType)
        {
            using (HttpClient client = CreateServiceBusHttpClient())
            {
                return client.GetJson<List<ConnectionDetail>>(
                    UriElement.ConnectionStringUri(namespaceName, entityName, entityType),
                    Logger);
            }
        }

        /// <summary>
        /// Gets all the available connection strings for given namespace.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <returns>List of connection strings</returns>
        public virtual List<ConnectionDetail> GetConnectionString(string namespaceName)
        {
            using (HttpClient client = CreateServiceBusHttpClient())
            {
                return client.GetJson<List<ConnectionDetail>>(
                    UriElement.ConnectionStringUri(namespaceName),
                    Logger);
            }
        }

        /// <summary>
        /// Creates new Windows authorization rule for Service Bus. This works on Windows Azure Pack on prim only.
        /// </summary>
        /// <param name="namespaceName">The service bus namespace name</param>
        /// <param name="ruleName">The authorization rule name</param>
        /// <param name="username">The user principle name</param>
        /// <param name="permissions">Set of permissions given to the rule</param>
        /// <returns>The created Windows authorization rule</returns>
        public virtual AllowRule CreateWindowsAuthorization(
            string namespaceName,
            string ruleName,
            string username,
            params AccessRights[] permissions)
        {
            AllowRule rule = new AllowRule(
                string.Empty,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn",
                username,
                permissions);

            using (HttpClient client = CreateServiceBusHttpClient())
            {
                rule = client.PostJson(UriElement.GetNamespaceAuthorizationRulesPath(namespaceName), rule, Logger);
            }

            return rule;
        }

        /// <summary>
        /// Creates shared access signature authorization for the service bus namespace. This authorization works on
        /// public Windows Azure environments and Windows Azure Pack on prim as well.
        /// </summary>
        /// <param name="namespaceName">The service bus namespace name</param>
        /// <param name="ruleName">The SAS authorization rule name</param>
        /// <param name="primaryKey">The SAS primary key. It'll be generated if empty</param>
        /// <param name="secondaryKey">The SAS secondary key</param>
        /// <param name="permissions">Set of permissions given to the rule</param>
        /// <returns>The created Shared Access Signature authorization rule</returns>
        public virtual ExtendedAuthorizationRule CreateSharedAccessAuthorization(
            string namespaceName,
            string ruleName,
            string primaryKey,
            string secondaryKey,
            params AccessRights[] permissions)
        {
            SharedAccessAuthorizationRule rule = new SharedAccessAuthorizationRule(
                ruleName,
                string.IsNullOrEmpty(primaryKey) ? SharedAccessAuthorizationRule.GenerateRandomKey() : primaryKey,
                secondaryKey,
                permissions);

            using (HttpClient client = CreateServiceBusHttpClient())
            {
                rule = client.PostJson(UriElement.GetNamespaceAuthorizationRulesPath(namespaceName), rule, Logger);
            }

            return CreateExtendedExtendedAuthorizationRule(rule, namespaceName);
        }

        /// <summary>
        /// Creates shared access signature authorization for the service bus entity. This authorization works on
        /// public Windows Azure environments and Windows Azure Pack on prim as well.
        /// </summary>
        /// <param name="namespaceName">The service bus namespace name</param>
        /// <param name="entityName">The fully qualified service bus entity name</param>
        /// <param name="entityType">The service bus entity type (e.g. Queue)</param>
        /// <param name="ruleName">The SAS authorization rule name</param>
        /// <param name="primaryKey">The SAS primary key. It'll be generated if empty</param>
        /// <param name="secondaryKey">The SAS secondary key</param>
        /// <param name="permissions">Set of permissions given to the rule</param>
        /// <returns>The created Shared Access Signature authorization rule</returns>
        public ExtendedAuthorizationRule CreateSharedAccessAuthorization(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType,
            string ruleName,
            string primaryKey,
            string secondaryKey,
            params AccessRights[] permissions)
        {
            // Create the SAS authorization rule
            SharedAccessAuthorizationRule rule = new SharedAccessAuthorizationRule(
                ruleName,
                string.IsNullOrEmpty(primaryKey) ? SharedAccessAuthorizationRule.GenerateRandomKey() : primaryKey,
                secondaryKey,
                permissions);

            // Create namespace manager
            NamespaceManager namespaceManager = CreateNamespaceManager(namespaceName);

            // Add the SAS rule and update the entity
            switch (entityType)
            {
                case ServiceBusEntityType.Queue:
                    QueueDescription queue = namespaceManager.GetQueue(entityName);
                    queue.Authorization.Add(rule);
                    namespaceManager.UpdateQueue(queue);
                    break;

                case ServiceBusEntityType.Topic:
                    TopicDescription topic = namespaceManager.GetTopic(entityName);
                    topic.Authorization.Add(rule);
                    namespaceManager.UpdateTopic(topic);
                    break;

                case ServiceBusEntityType.Relay:
                    RelayDescription relay = namespaceManager.GetRelayAsync(entityName).Result;
                    relay.Authorization.Add(rule);
                    namespaceManager.UpdateRelayAsync(relay).Wait();
                    break;

                case ServiceBusEntityType.NotificationHub:
                    NotificationHubDescription notificationHub = namespaceManager.GetNotificationHub(entityName);
                    notificationHub.Authorization.Add(rule);
                    namespaceManager.UpdateNotificationHub(notificationHub);
                    break;

                default:
                    throw new Exception(string.Format(Resources.ServiceBusEntityTypeNotFound, entityType.ToString()));
            }
            
            return CreateExtendedExtendedAuthorizationRule(rule, namespaceName, entityName, entityType);
        }

        /// <summary>
        /// Creates new service bus queue in the given name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="name">The queue name</param>
        /// <returns>The queue description object</returns>
        public QueueDescription CreateQueue(string namespaceName, string name)
        {
            return CreateNamespaceManager(namespaceName).CreateQueue(name);
        }

        /// <summary>
        /// Creates new service bus topic in the given name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="name">The topic name</param>
        /// <returns>The topic description object</returns>
        public TopicDescription CreateTopic(string namespaceName, string name)
        {
            return CreateNamespaceManager(namespaceName).CreateTopic(name);
        }

        /// <summary>
        /// Creates new service bus relay in the given name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="name">The relay name</param>
        /// <param name="type">The relay type</param>
        /// <returns>The relay description object</returns>
        public RelayDescription CreateRelay(string namespaceName, string name, RelayType type)
        {
            return CreateNamespaceManager(namespaceName).CreateRelayAsync(name, type).Result;
        }

        /// <summary>
        /// Creates new service bus notification hub in the given name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="name">The notification hub name</param>
        /// <returns>The notification hub description object</returns>
        public NotificationHubDescription CreateNotificationHub(string namespaceName, string name)
        {
            NotificationHubDescription description = new NotificationHubDescription(name);
            return CreateNamespaceManager(namespaceName).CreateNotificationHub(description);
        }

        /// <summary>
        /// Gets all available SAS authorization rules for given namespace on namespace scope.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <returns>The collection of authorization rules</returns>
        public List<SharedAccessAuthorizationRule> GetSharedAccessAuthorizationRule(string namespaceName)
        {
            return GetAuthorizationRuleCore<SharedAccessAuthorizationRule>(namespaceName);
        }

        /// <summary>
        /// Gets all available Windows authorization rules for given namespace on namespace scope.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <returns>The collection of authorization rules</returns>
        public List<AllowRule> GetWindowsAuthorizationRule(string namespaceName)
        {
            return GetAuthorizationRuleCore<AllowRule>(namespaceName);
        }

        /// <summary>
        /// Gets all available SAS authorization rules for given entity.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="entityName">The entity name</param>
        /// <param name="entityType">The entity type</param>
        /// <returns>The list of SAS authorization rules</returns>
        public List<ExtendedAuthorizationRule> GetSharedAccessAuthorizationRule(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType)
        {
            return GetAuthorizationRuleCore(
                namespaceName,
                entityName,
                entityType,
                r => r is SharedAccessAuthorizationRule);
        }

        /// <summary>
        /// Gets the specified authorization rule name on namespace scope.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="name">The authorization rule name</param>
        /// <param name="type">The authorization rule type</param>
        /// <returns></returns>
        public ExtendedAuthorizationRule GetAuthorizationRule(string namespaceName, string name, AuthorizationType type)
        {
            AuthorizationRule rule = null;

            switch (type)
            {
                case AuthorizationType.WindowsAuthorization:
                    rule = GetWindowsAuthorizationRule(namespaceName).FirstOrDefault(r => r.KeyName.Equals(
                        name,
                        StringComparison.OrdinalIgnoreCase));
                    break;

                case AuthorizationType.SharedAccessAuthorization:
                    rule = GetSharedAccessAuthorizationRule(namespaceName).FirstOrDefault(r => r.KeyName.Equals(
                        name,
                        StringComparison.OrdinalIgnoreCase));
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return CreateExtendedExtendedAuthorizationRule(rule, namespaceName);
        }

        /// <summary>
        /// Gets authorization rule with the specified name.
        /// </summary>
        /// <param name="namespaceName">The namespace name</param>
        /// <param name="entityName">The entity name</param>
        /// <param name="entityType">The</param>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        public ExtendedAuthorizationRule GetAuthorizationRule(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType,
            string ruleName)
        {
            List<ExtendedAuthorizationRule> match = GetAuthorizationRuleCore(
                namespaceName,
                entityName,
                entityType,
                r => r.KeyName.Equals(ruleName, StringComparison.OrdinalIgnoreCase));

            return match.FirstOrDefault();
        }

        /// <summary>
        /// Updates shared access signature authorization for the service bus namespace. This authorization works on
        /// public Windows Azure environments and Windows Azure Pack on prim as well.
        /// </summary>
        /// <param name="namespaceName">The service bus namespace name</param>
        /// <param name="ruleName">The SAS authorization rule name</param>
        /// <param name="primaryKey">The SAS primary key. It'll be generated if empty</param>
        /// <param name="secondaryKey">The SAS secondary key</param>
        /// <param name="permissions">Set of permissions given to the rule</param>
        /// <returns>The created Shared Access Signature authorization rule</returns>
        public virtual ExtendedAuthorizationRule UpdateSharedAccessAuthorization(
            string namespaceName,
            string ruleName,
            string primaryKey,
            string secondaryKey,
            params AccessRights[] permissions)
        {
            SharedAccessAuthorizationRule rule = (SharedAccessAuthorizationRule)GetAuthorizationRule(
                namespaceName,
                ruleName,
                AuthorizationType.SharedAccessAuthorization).Rule;

            // Update the rule
            rule.Rights = permissions ?? rule.Rights;
            rule.PrimaryKey = string.IsNullOrEmpty(primaryKey) ? rule.PrimaryKey : primaryKey;
            rule.SecondaryKey = string.IsNullOrEmpty(secondaryKey) ? rule.SecondaryKey : secondaryKey;

            // In case that there's nothing to update then assume user asks for primary key renewal
            if (permissions == null && string.IsNullOrEmpty(secondaryKey) && string.IsNullOrEmpty(primaryKey))
            {
                rule.PrimaryKey = SharedAccessAuthorizationRule.GenerateRandomKey();
            }

            using (HttpClient client = CreateServiceBusHttpClient())
            {
                rule = client.PutJson(UriElement.GetAuthorizationRulePath(namespaceName, ruleName), rule, Logger);
            }

            return CreateExtendedExtendedAuthorizationRule(rule, namespaceName);
        }

        /// <summary>
        /// Updates shared access signature authorization for the service bus entity. This authorization works on
        /// public Windows Azure environments and Windows Azure Pack on prim as well.
        /// </summary>
        /// <param name="namespaceName">The service bus namespace name</param>
        /// <param name="entityName">The fully qualified service bus entity name</param>
        /// <param name="entityType">The service bus entity type (e.g. Queue)</param>
        /// <param name="ruleName">The SAS authorization rule name</param>
        /// <param name="primaryKey">The SAS primary key. It'll be generated if empty</param>
        /// <param name="secondaryKey">The SAS secondary key</param>
        /// <param name="permissions">Set of permissions given to the rule</param>
        /// <returns>The created Shared Access Signature authorization rule</returns>
        public ExtendedAuthorizationRule UpdateSharedAccessAuthorization(
            string namespaceName,
            string entityName,
            ServiceBusEntityType entityType,
            string ruleName,
            string primaryKey,
            string secondaryKey,
            params AccessRights[] permissions)
        {
            bool removed = false;
            SharedAccessAuthorizationRule oldRule = (SharedAccessAuthorizationRule)GetAuthorizationRule(
                namespaceName,
                entityName,
                entityType,
                ruleName).Rule;

            SharedAccessAuthorizationRule newRule = new SharedAccessAuthorizationRule(
                ruleName,
                string.IsNullOrEmpty(primaryKey) ? SharedAccessAuthorizationRule.GenerateRandomKey() : primaryKey,
                secondaryKey,
                permissions ?? oldRule.Rights);

            // Create namespace manager
            NamespaceManager namespaceManager = CreateNamespaceManager(namespaceName);

            // Add the SAS rule and update the entity
            switch (entityType)
            {
                case ServiceBusEntityType.Queue:
                    QueueDescription queue = namespaceManager.GetQueue(entityName);
                    removed = queue.Authorization.Remove(oldRule);
                    Debug.Assert(removed);
                    queue.Authorization.Add(newRule);
                    namespaceManager.UpdateQueue(queue);
                    break;

                case ServiceBusEntityType.Topic:
                    TopicDescription topic = namespaceManager.GetTopic(entityName);
                    removed = topic.Authorization.Remove(oldRule);
                    Debug.Assert(removed);
                    topic.Authorization.Add(newRule);
                    namespaceManager.UpdateTopic(topic);
                    break;

                case ServiceBusEntityType.Relay:
                    RelayDescription relay = namespaceManager.GetRelayAsync(entityName).Result;
                    removed = relay.Authorization.Remove(oldRule);
                    Debug.Assert(removed);
                    relay.Authorization.Add(newRule);
                    namespaceManager.UpdateRelayAsync(relay).Wait();
                    break;

                case ServiceBusEntityType.NotificationHub:
                    NotificationHubDescription notificationHub = namespaceManager.GetNotificationHub(entityName);
                    removed = notificationHub.Authorization.Remove(oldRule);
                    Debug.Assert(removed);
                    notificationHub.Authorization.Add(newRule);
                    namespaceManager.UpdateNotificationHub(notificationHub);
                    break;

                default:
                    throw new Exception(string.Format(Resources.ServiceBusEntityTypeNotFound, entityType.ToString()));
            }

            return CreateExtendedExtendedAuthorizationRule(newRule, namespaceName, entityName, entityType);
        }
    }

    public enum ServiceBusEntityType
    {
        Queue,
        Topic,
        Relay,
        NotificationHub
    }

    public enum AuthorizationType
    {
        SharedAccessAuthorization,
        WindowsAuthorization
    }
}
