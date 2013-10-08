namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using System;
    using System.Collections.Generic;

    public abstract class OperationsBase<T> where T : class
    {
        /// <summary>
        /// Submits request for creating object of type T. Returns "temporary" or "future" object if request is submitted successfully.
        /// Caller must wait on jobId to track the status of the operation.
        /// </summary>
        /// <param name="toCreate"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public virtual T Create(T toCreate, out Guid jobId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a resource by ID. 
        /// </summary>
        /// <param name="id">Unique GUID of the resource to be returned</param>
        /// <returns>Resouce object (e.g., VirtualMachine, VirtualHardDisk, etc.)</returns>
        public virtual T Read(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a list of all resources of type T.
        /// </summary>
        /// <returns></returns>
        public virtual List<T> Read()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits request for updating a resource (arguments's ID must match the existing resource's ID).
        /// Returns the temp object.
        /// Caller must wait on jobId to track the status of the operation.
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public virtual T Update(T toUpdate, out Guid jobId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits request for deleting resource with given Id.
        /// Returns true if deletion request is submitted successfully.
        /// Caller must wait on jobId to track the status of the operation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public virtual bool Delete(Guid id, out Guid jobId)
        {
            throw new NotImplementedException();
        }
    }
}
