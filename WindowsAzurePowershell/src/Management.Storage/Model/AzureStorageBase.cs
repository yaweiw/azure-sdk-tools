
namespace Microsoft.WindowsAzure.Management.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class AzureStorageBase
    {
        public StorageContext Context { get; set; }
        public String Name { get; set; }
    }
}
