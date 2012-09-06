//------------------------------------------------------------------------------
// <copyright file="UriElements.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api
{

    public class UriElements
    {
        public const string ServiceNamespace = "http://schemas.microsoft.com/windowsazure";

        public const string Plans = "plans";
        public const string Subscriptions = "subscriptions";
        public const string Web = "web";
        public const string WebAdmin = "webadmin";
        public const string MySql = "mysql";
        public const string SqlServer = "sqlserver";
        public const string SqlAzure = "sqlAzure";
        public const string Users = "users";
        public const string ResourceProvider = "resourceprovider";
        public const string MetricDefinitions = "metricdefinitions";
        public const string Authentication = "authentication";
        public const string GeoAdmin = "geoadmin";
        public const string AccessControl = "accesscontrol";
        public const string Capacity = "capacities";
        public const string Offers = "offers";

        // Parameters
        public const string NameTemplateParameter = "/{name}";
        public const string UserNameTemplateParameter = "/{userName}";
        public const string CommandTemplateParameter = "?comp=command";
        public const string UsersPublishAuthN = "?publishauthenticated&source={source}&protocol={protocol}&userAddress={userAddress}";
        public const string UsersPublishAuthZ = "?publishauthorized&userName={publishingUserName}&authorizedSite={siteName}&repository={isrepository}&source={source}&protocol={protocol}&userAddress={userAddress}";
        public const string DeleteMetricsParameter = "?deleteMetrics={deleteMetrics}";
        public const string ExistsParameter = "/exists";
        public const string MetricsParameters = "?names={metrics}&startTime={startTime}&endTime={endTime}";
        public const string ComputeMode = "?computeMode={computeMode}&siteMode={siteMode}&enforcementScope={enforcementScope}";
        public const string PolicyParameters = "?computeMode={computeMode}&siteMode={siteMode}";
        public const string SkipValidationParameter = "?skipValidation={skipValidation}";
        public const string PaginationParameters = "?pageNumber={pageNumber}&pageSize={pageSize}&filter={filter}";
        public const string ContinuationParameters = "?marker={marker}&recordCount={recordCount}";
        public const string AllowPendingStateParameter = "?allowPendingState={allowPendingState}";
        public const string PropertiesToIncludeParameter = "?propertiesToInclude={propertiesToInclude}";
        public const string ListOnlyOnlineStampsParameter = "?listOnlyOnline={listOnlyOnline}";
        public const string BindingsParameter = "?bindings={bindings}";
        public const string BindingParameter = "/{ip}/{port}";

        public const string RepositoryUriProperty = "RepositoryUri";
        public const string PublishingUsernameProperty = "PublishingUsername";
        public const string PublishingPasswordProperty = "PublishingPassword";
        public const string MetadataProperty = "Metadata";

        // Service resources
        public const string Root = "";
        public const string RolesRoot = "roles";
        public const string EntitiesRoot = "entities";
        public const string UserRoles = "/userRoles";
        public const string CloudEntityRoot = EntitiesRoot + UserRoles;
        public const string SubscriptionEntitiesRoot = EntitiesRoot + "/subscriptions/{subscriptionName}" + UserRoles;
        public const string WebSpaceEntitiesRoot = EntitiesRoot + "/subscriptions/{subscriptionName}/webspaces/{webspaceName}" + UserRoles;
        public const string WebSiteEntitiesRoot = EntitiesRoot + "/subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{siteName}" + UserRoles;
        public const string GeoRegionsRoot = "regions";
        public const string GeoLocationsRoot = "regions/{regionName}/locations";
        public const string StampsRoot = "locations/{locationName}/stamps";
        public const string StampsCommand = "?Command={command}";
        public const string WebSitesRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites";
        public const string SqlDatabasesRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sqldbs";
        public const string MySqlDatabaseRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/mysqldbs";
        public const string SqlAzureDatabasesRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sqlazuredbs";
        public const string WebSpacesRoot = "{subscriptionName}/webspaces";
        public const string WebSpaceUsagesRoot = "{subscriptionName}/webspaces/{webspaceName}/usages?names={usages}&computeMode={computeMode}&siteMode={siteMode}";
        public const string WebSiteUsagesRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/usages?names={usages}&computeMode={computeMode}&siteMode={siteMode}";
        public const string WebSiteMetricsRoot = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/metrics";
        public const string WebSiteMetricDefinitions = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/metricdefinitions";
        public const string WebSiteConfig = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/config";
        public const string WebSiteRepository = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/repository";
        public const string WebSiteRepositoryDev = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/repository/dev";
        public const string WebSiteAuditLogs = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/auditlogs?startTime={startTime}&endTime={endTime}";
        public const string WebSiteGetLastAuditLog = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/lastauditlog";
        public const string WebSiteSwap = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}?Command={command}&OtherSiteName={otherSiteName}";
        public const string WebSiteRestart = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/restart";
        public const string WebSiteIsValidCustomDomain = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/isvalidcustomdomain?hostName={hostName}&type={recordType}";

        public const string HostNameAvailability = "ishostnameavailable/{subDomain}";

        public const string HostNameReservedOrNotAllowed = "ishostnamereservedornotallowed/{subDomain}";

        public const string WebSitesPerSubscription = "subscriptions/{subscriptionName}/sites";
        public const string SqlDatabasesPerSubscription = "subscriptions/{subscriptionName}/sqldbs";
        public const string MySqlDatabasesPerSubscription = "subscriptions/{subscriptionName}/mysqldbs";

        public const string SubscriptionPublishingUsers = "{subscriptionName}/publishingUsers";

        public const string WebSitePublishingProfile = "subscriptions/{subscriptionName}/webspaces/{webspaceName}/sites/{name}/publishxml";

        public const string RDFENotification = "notification";

        public const string Systems = "systems";
        public const string WebWorkers = "systems/{webSystemName}/webworkers";
        public const string LoadBalancers = "systems/{webSystemName}/loadbalancers";
        public const string LoadBalancerSslBindings = "systems/{webSystemName}/loadbalancers/{name}/sslbindings";
        public const string Publishers = "systems/{webSystemName}/publishers";
        public const string Controllers = "systems/{webSystemName}/controllers";
        public const string FileServers = "systems/{webSystemName}/fileservers";
        public const string ManagementServers = "systems/{webSystemName}/managementservers";
        public const string WebPlan = "/{name}/web";
        public const string WebQuotas = "/{planName}/web/quotas";
        public const string Policies = "/{planName}/web/policies";
        public const string SystemSites = "systems/{name}/sites?filter={filter}&pageNumber={pageNumber}&pageSize={pageSize}&details={details}&orderBy={orderBy}";
        public const string Credentials = "systems/{webSystemName}/credentials";

        public const string SystemMetrics = "systems/{name}/metrics";
        public const string WebWorkerMetrics = "systems/{webSystemName}/webworkers/{name}/metrics";
        public const string LoadBalancerMetrics = "systems/{webSystemName}/loadbalancers/{name}/metrics";
        public const string PublisherMetrics = "systems/{webSystemName}/publishers/{name}/metrics";
        public const string ControllerMetrics = "systems/{webSystemName}/controllers/{name}/metrics";
        public const string FileServerMetrics = "systems/{webSystemName}/fileservers/{name}/metrics";
        public const string ManagementServerMetrics = "systems/{webSystemName}/managementservers/{name}/metrics";

        public const string WorkerSites = "systems/{webSystemName}/webworkers/{name}/sites?filter={filter}&pageNumber={pageNumber}&pageSize={pageSize}&details={details}&orderBy={orderBy}";

        public const string SystemSettings = "systems/{name}/config";
        public const string SystemLog = "systems/{name}/log";
        public const string SystemSummary = "systems/{name}/summary";

        public const string WebWorkerRole = "webworkers";
        public const string LoadBalancerRole = "loadbalancers";
        public const string PublisherRole = "publishers";
        public const string ControllerRole = "controllers";
        public const string FileServerRole = "fileservers";
        public const string ManagementServerRole = "managementservers";

        public const string CleanAuditLogs = "auditlogs?timestamp={timestamp}";

        // This is for checking that the URL using {role} parameter is valid according to the specification
        public static readonly string[] AvailableRoles = new string[] { WebWorkerRole, LoadBalancerRole, PublisherRole, ControllerRole };

        public const string PutMachineOffline = "systems/{webSystemName}/{role}/{name}/offline";
        public const string PutMachineOnline = "systems/{webSystemName}/{role}/{name}/online";
        public const string RebootMachine = "systems/{webSystemName}/{role}/{name}/reboot";
        public const string RepairMachine = "systems/{webSystemName}/{role}/{name}/repair";
        public const string MachineLog = "systems/{webSystemName}/{role}/{name}/log";
        public const string IsMachineValid = "systems/{webSystemName}/{role}/{name}/valid";

        public const string DatabaseCheckAvailability = "databases/{name}?CheckAvailability";
        public const string WebSiteCheckAvailability = "sites/{name}?CheckAvailability";
    }
}