namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;

    public class JobOperationsBase : OperationsBase<Job>
    {
        public override Job Read(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Waits until the job with the given ID is completed, then returns the job object.
        /// Default timeout (-1) is unlimited, but can be limited by specifying a value in milliseconds. 
        /// If timeout is reached before the job is finished, the latest version of the job object is returned.
        /// If timeout is a possibility, caller must check the returned job object to determine whether the job finished or timeout occurred.
        /// </summary>
        /// <param name="jobId">GUID of the job to wait on</param>
        /// <param name="timeout">Duration of time, in milliseconds, to wait for the job before returning. -1 means unlimited.</param>
        /// <returns>Job object</returns>
        public Job WaitOnJob(Guid jobId, long timeout = -1)
        {
            throw new NotImplementedException();
        }
    }
}
