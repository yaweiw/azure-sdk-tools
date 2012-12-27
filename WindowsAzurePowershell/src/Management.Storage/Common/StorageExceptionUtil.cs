namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// storage exception utility
    /// </summary>
    public static class StorageExceptionUtil
    {
        /// <summary>
        /// whether the storage exception is resource not found exception or not.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsNotFoundException(this StorageException e)
        {
            return e.RequestInformation != null && e.RequestInformation.HttpStatusCode == 404;
        }

        /// <summary>
        /// repace storage exception to expose more information in Message.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static StorageException RepackStorageException(this StorageException e)
        {
            if (null != e.RequestInformation &&
                null != e.RequestInformation.HttpStatusMessage)
            {
                String msg = string.Format(
                    Resources.StorageExceptionDetails,
                    e.Message,
                    e.RequestInformation.HttpStatusCode,
                    e.RequestInformation.HttpStatusMessage);
                e = new StorageException(e.RequestInformation, msg, e);
            }
            return e;
        }
    }
}
