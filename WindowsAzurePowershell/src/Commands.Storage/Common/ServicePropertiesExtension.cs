namespace Microsoft.WindowsAzure.Commands.Storage.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    public static class ServicePropertiesExtension
    {
        /// <summary>
        /// Clean all the settings on the ServiceProperties project
        /// </summary>
        /// <param name="serviceProperties">Service properties</param>
        internal static void Clean(this ServiceProperties serviceProperties)
        {
            serviceProperties.Logging = null;
            serviceProperties.HourMetrics = null;
            serviceProperties.MinuteMetrics = null;
            serviceProperties.Cors = null;
            serviceProperties.DefaultServiceVersion = null;
        }
    }
}
