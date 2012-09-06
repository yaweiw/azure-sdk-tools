//------------------------------------------------------------------------------
// <copyright file="ApiConstants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api
{
    public static class ApiConstants
    {
        public static string DefaultApiVersion = "2011-03-01";

        public const string VersionHeaderName = "x-ms-version";
        public const string RequestHeaderName = "x-ms-request-id";
        public const string MarkerHeaderName = "x-ms-continuation-Marker";

        public const string TracingHeaderName = "x-ms-tracing";

        public const string TracingEventResponseHeaderPrefix = "TracingEvent_";

        public const string AuthorizationHeaderName = "Authorization";

        public const string RunningState = "Running";
        public const string StoppedState = "Stopped";

        public const string CustomDomainsEnabledSettingsName = "CustomDomainsEnabled";
        public const string SslSupportSettingsName = "SslSupport";
    }
}
