using Microsoft.WindowsAzure.Commands.Utilities.Websites.Services.WebEntities;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Commands.Utilities.MediaServices.Services.Entities
{
    /// <summary>
    ///     Paged collection of sites
    /// </summary>
    [DataContract(Namespace = MediaServicesUriElements.ServiceNamespace)]
    public class PagedSites : PagedSet<MediaServiceAccount>
    {
    }
}