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

namespace Microsoft.WindowsAzure.Management.Websites.Utilities
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
