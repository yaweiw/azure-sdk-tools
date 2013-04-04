using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Management.Utilities.Subscriptions.Contract
{
    using System.Threading.Tasks;

    public interface ISubscriptionClient
    {
        /// <summary>
        /// Get a list of resources that are registered for this subscription
        /// </summary>
        /// <param name="knownResourceTypes">Resource types to query for.</param>
        /// <returns></returns>
        Task<IEnumerable<ProviderResource>> ListResourcesAsync(IEnumerable<string> knownResourceTypes);
    }

    public static class SubscriptionClientExtensions
    {
        /// <summary>
        /// Synchronously get a list of resources that are registered for this subscription
        /// </summary>
        /// <param name="client"></param>
        /// <param name="knownResourceTypes">Resource types to query for.</param>
        /// <returns>Sequence of the state of each of the requested resources.</returns>
        public static IEnumerable<ProviderResource> ListResources(this ISubscriptionClient client,
            IEnumerable<string> knownResourceTypes)
        {
            return client.ListResourcesAsync(knownResourceTypes).Result;
        }
    }
}
