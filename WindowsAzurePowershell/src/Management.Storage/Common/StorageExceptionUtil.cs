namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class StorageExceptionUtil
    {
        public static bool IsNotFoundException(StorageException e)
        {
            return e.RequestInformation != null && e.RequestInformation.HttpStatusCode == 404;
        }

        public static StorageException RepackStorageException(StorageException e)
        {
            if (null != e.RequestInformation &&
                null != e.RequestInformation.HttpStatusMessage)
            {
                String msg = string.Format(
                    "{0}. HTTP Status Code: {1} - HTTP Error Message: {2}",
                    e.Message,
                    e.RequestInformation.HttpStatusCode,
                    e.RequestInformation.HttpStatusMessage);
                e = new StorageException(e.RequestInformation, msg, e);
            }
            return e;
        }
    }
}
