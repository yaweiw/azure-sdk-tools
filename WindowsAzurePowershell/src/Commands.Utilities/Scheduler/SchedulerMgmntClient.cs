// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler
{
    using Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model;
    using Microsoft.WindowsAzure.Management.Scheduler;
    using Microsoft.WindowsAzure.Management.Scheduler.Models;
    using Microsoft.WindowsAzure.Scheduler;
    using Microsoft.WindowsAzure.Scheduler.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.Common;

    public partial class SchedulerMgmntClient
    {
        private SchedulerManagementClient schedulerManagementClient;
        private CloudServiceManagementClient csmClient;
        private const string SupportedRegionsKey = "SupportedGeoRegions";
      
        /// <summary>
        /// Creates new Scheduler Management Convenience Client
        /// </summary>
        /// <param name="subscription">Subscription containing websites to manipulate</param>
        public SchedulerMgmntClient(WindowsAzureSubscription subscription)
        {
            csmClient = subscription.CreateClient<CloudServiceManagementClient>();
            schedulerManagementClient = subscription.CreateClient<SchedulerManagementClient>();
        }

        #region Get Available Regions
        public List<string> GetAvailableRegions()
        {
            List<string> returnLst = new List<string>();
            IDictionary<string, string> dict=  schedulerManagementClient.GetResourceProviderProperties().Properties;
            dict[SupportedRegionsKey].Split(',').ToList().ForEach(s => returnLst.Add(s));
            return returnLst;
        }
        #endregion

        #region Job Collections
        public List<PSJobCollection> GetJobCollection(string region = "", string jobCollection = "")
        {
            List<PSJobCollection> lstSchedulerJobCollection = new List<PSJobCollection>();
            CloudServiceListResponse csList = csmClient.CloudServices.List();
            
            if (!string.IsNullOrEmpty(region))
            {
                string cloudService = region.ToCloudServiceName();
                foreach (CloudServiceListResponse.CloudService cs in csList.CloudServices)
                {
                    if (cs.Name.Equals(cloudService, StringComparison.OrdinalIgnoreCase))
                    {
                        GetSchedulerJobCollection(cs, jobCollection).ForEach(x => lstSchedulerJobCollection.Add(x));
                        //If job collection parameter was passed and we found a matching job collection already, exit out of the loop and return the job collection
                        if (!string.IsNullOrEmpty(jobCollection) && lstSchedulerJobCollection.Count > 0)
                            return lstSchedulerJobCollection;
                    }
                }
            }
            else if (string.IsNullOrEmpty(region))
            {
                foreach (CloudServiceListResponse.CloudService cs in csList.CloudServices)
                {
                    if (cs.Name.Equals(Constants.CloudServiceNameFirst + cs.GeoRegion.Replace(" ", string.Empty) + Constants.CloudServiceNameSecond, StringComparison.OrdinalIgnoreCase))
                    {
                        GetSchedulerJobCollection(cs, jobCollection).ForEach(x => lstSchedulerJobCollection.Add(x));
                        //If job collection parameter was passed and we found a matching job collection already, exit out of the loop and return the job collection
                        if (!string.IsNullOrEmpty(jobCollection) && lstSchedulerJobCollection.Count > 0)
                            return lstSchedulerJobCollection;
                    }
                }
            }
            return lstSchedulerJobCollection;
        }

        private List<PSJobCollection> GetSchedulerJobCollection(CloudServiceListResponse.CloudService cloudService, string jobCollection)
        {
            List<PSJobCollection> lstSchedulerJobCollection = new List<PSJobCollection>();
            
            foreach (CloudServiceGetResponse.Resource csRes in csmClient.CloudServices.Get(cloudService.Name).Resources)
            {
                if (csRes.Type.Contains(Constants.JobCollectionResource))
                {
                    JobCollectionGetResponse jcGetResponse = schedulerManagementClient.JobCollections.Get(cloudService.Name, csRes.Name);
                    if (string.IsNullOrEmpty(jobCollection) || (!string.IsNullOrEmpty(jobCollection) && jcGetResponse.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase)))
                    {
                        lstSchedulerJobCollection.Add(new PSJobCollection
                        {
                            CloudServiceName = cloudService.Name,
                            JobCollectionName = jcGetResponse.Name,
                            MaxJobCount = jcGetResponse.IntrinsicSettings.Quota.MaxJobCount.ToString(),
                            MaxRecurrence = jcGetResponse.IntrinsicSettings.Quota.MaxRecurrence == null ? "" : jcGetResponse.IntrinsicSettings.Quota.MaxRecurrence.Interval.ToString() + " per " +
                                jcGetResponse.IntrinsicSettings.Quota.MaxRecurrence.Frequency.ToString(),
                            State = Enum.GetName(typeof(JobCollectionState), jcGetResponse.State),
                            Plan = Enum.GetName(typeof(JobCollectionPlan), jcGetResponse.IntrinsicSettings.Plan),
                            Location = cloudService.GeoRegion,
                            Uri = csmClient.BaseUri.AbsoluteUri + csmClient.Credentials.SubscriptionId + "cloudservices/" + cloudService.Name + Constants.JobCollectionResourceURL + jcGetResponse.Name
                        });
                    }
                }
            }
          
            return lstSchedulerJobCollection;
        }

        #endregion

        #region Scheduler Jobs

        public List<PSSchedulerJob> GetJob(string region, string jobCollection, string job="", string state="")
        {
            List<PSSchedulerJob> lstJob = new List<PSSchedulerJob>();

            string cloudService = region.ToCloudServiceName();
            if (!string.IsNullOrEmpty(job))
            {
                PSJobDetail jobDetail = GetJobDetail(jobCollection, job, cloudService);
                if (string.IsNullOrEmpty(state) || (!string.IsNullOrEmpty(state) && jobDetail.Status.Equals(state, StringComparison.OrdinalIgnoreCase)))
                {
                    lstJob.Add(jobDetail);
                    return lstJob;
                }
            }
            else if (string.IsNullOrEmpty(job))
            {
                GetSchedulerJobs(cloudService, jobCollection).ForEach(x =>
                {
                    if (string.IsNullOrEmpty(state) || (!string.IsNullOrEmpty(state) && x.Status.Equals(state, StringComparison.OrdinalIgnoreCase)))
                    {
                        lstJob.Add(x);
                    }
                });
            }
            return lstJob;
        }

        private List<PSSchedulerJob> GetSchedulerJobs(string cloudService, string jobCollection)
        {
            List<PSSchedulerJob> lstJobs = new List<PSSchedulerJob>();
            CloudServiceGetResponse csDetails = csmClient.CloudServices.Get(cloudService);
            foreach (CloudServiceGetResponse.Resource csRes in csDetails.Resources)
            {
                if (csRes.ResourceProviderNamespace.Equals(Constants.SchedulerRPNameProvider, StringComparison.OrdinalIgnoreCase) && csRes.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase))
                {
                    SchedulerClient schedClient = new SchedulerClient(csmClient.Credentials, cloudService, jobCollection);
                    JobListResponse jobs = schedClient.Jobs.List(new JobListParameters
                    {
                        Skip = 0,
                    });
                    foreach (Job job in jobs)
                    {
                        lstJobs.Add(new PSSchedulerJob
                        {
                            JobName = job.Id,
                            Lastrun = job.Status == null ? null : job.Status.LastExecutionTime,
                            Nextrun = job.Status == null ? null : job.Status.NextExecutionTime,
                            Status = job.State.ToString(),
                            StartTime = job.StartTime,
                            Recurrence = job.Recurrence == null ? "" : job.Recurrence.Interval.ToString() + " per " + job.Recurrence.Frequency.ToString(),
                            Failures = job.Status == null ? default(int?) : job.Status.FailureCount,
                            Faults = job.Status == null ? default(int?) : job.Status.FaultedCount,
                            Executions = job.Status == null ? default(int?) : job.Status.ExecutionCount,
                            EndSchedule = GetEndTime(job),
                            JobCollectionName = jobCollection
                        });
                    }
                }
            }
            return lstJobs;
        }

        private string GetEndTime(Job job)
        {
            if (job.Recurrence == null)
                return "Run once";
            else if (job.Recurrence != null)
            {
                if (job.Recurrence.Count == null)
                    return "None";
                if (job.Recurrence.Count != null)
                    return "Until " + job.Recurrence.Count + " executions";
                else
                    return job.Recurrence.Interval + " executions every " + job.Recurrence.Frequency.ToString();
            }
            return null;
        }

        #endregion

        #region Job History
        public List<PSJobHistory> GetJobHistory(string jobCollection, string job, string region, string jobStatus = "")
        {
            List<PSJobHistory> lstPSJobHistory = new List<PSJobHistory>();
            string cloudService = region.ToCloudServiceName();
            CloudServiceGetResponse csDetails = csmClient.CloudServices.Get(cloudService);
            foreach (CloudServiceGetResponse.Resource csRes in csDetails.Resources)
            {
                if (csRes.ResourceProviderNamespace.Equals(Constants.SchedulerRPNameProvider, StringComparison.InvariantCultureIgnoreCase) && csRes.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase))
                {
                    SchedulerClient schedClient = new SchedulerClient(csmClient.Credentials, cloudService, jobCollection.Trim());
                    List<JobGetHistoryResponse.JobHistoryEntry> lstHistory = new List<JobGetHistoryResponse.JobHistoryEntry>();
                    int currentTop = 100;

                    if(string.IsNullOrEmpty(jobStatus))
                    {
                        JobGetHistoryResponse history = schedClient.Jobs.GetHistory(job.Trim(), new JobGetHistoryParameters { Top = 100});
                        lstHistory.AddRange(history.JobHistory);
                        while(history.JobHistory.Count > 99)
                        {
                            history = schedClient.Jobs.GetHistory(job.Trim(), new JobGetHistoryParameters { Top = 100, Skip = currentTop});
                            currentTop+= 100;
                            lstHistory.AddRange(history.JobHistory);
                        }
                    }
                    else if(!string.IsNullOrEmpty(jobStatus))
                    {
                        JobHistoryStatus status = jobStatus.Equals("Completed") ? JobHistoryStatus.Completed : JobHistoryStatus.Failed;
                        JobGetHistoryResponse history = schedClient.Jobs.GetHistoryWithFilter(job.Trim(), new JobGetHistoryWithFilterParameters { Top = 100, Status = status});
                        lstHistory.AddRange(history.JobHistory);
                        while(history.JobHistory.Count > 99)
                        {
                            history = schedClient.Jobs.GetHistoryWithFilter(job.Trim(), new JobGetHistoryWithFilterParameters { Top = 100, Skip = currentTop });
                            currentTop+= 100;
                            lstHistory.AddRange(history.JobHistory);
                        }
                    }
                    foreach (JobGetHistoryResponse.JobHistoryEntry entry in lstHistory)
                    {
                        PSJobHistory historyObj = new PSJobHistory();
                        historyObj.Status = entry.Status.ToString();
                        historyObj.StartTime = entry.StartTime;
                        historyObj.EndTime = entry.EndTime;
                        historyObj.JobName = entry.Id;
                        historyObj.Details = GetHistoryDetails(entry.Message);
                        historyObj.Retry = entry.RetryCount;
                        historyObj.Occurence = entry.RepeatCount;
                        if (JobHistoryActionName.ErrorAction == entry.ActionName)
                        {
                            PSJobHistoryError errorObj = historyObj.ToJobHistoryError();
                            errorObj.ErrorAction = JobHistoryActionName.ErrorAction.ToString();
                            lstPSJobHistory.Add(errorObj);
                        }
                        else
                            lstPSJobHistory.Add(historyObj); 
                    }
                }
            }
            return lstPSJobHistory;
        }

        private PSJobHistoryDetail GetHistoryDetails(string message)
        {
            PSJobHistoryDetail detail = new PSJobHistoryDetail();
            if (message.Contains("Http Action -"))
            {
                PSJobHistoryHttpDetail details = new PSJobHistoryHttpDetail { ActionType = "http" };
                if (message.Contains("Request to host") && message.Contains("failed:"))
                {
                    int firstIndex = message.IndexOf("'");
                    int secondIndex = message.IndexOf("'", firstIndex + 1);
                    details.HostName = message.Substring(firstIndex + 1, secondIndex - (firstIndex + 1));
                    details.Response = "Failed";
                    details.ResponseBody = message;
                }
                else
                {
                    int firstIndex = message.IndexOf("'");
                    int secondIndex = message.IndexOf("'", firstIndex + 1);
                    int thirdIndex = message.IndexOf("'", secondIndex + 1);
                    int fourthIndex = message.IndexOf("'", thirdIndex + 1);
                    details.HostName = message.Substring(firstIndex + 1, secondIndex - (firstIndex + 1));
                    details.Response = message.Substring(thirdIndex + 1, fourthIndex - (thirdIndex + 1));
                    int bodyIndex = message.IndexOf("Body: ");
                    details.ResponseBody = message.Substring(bodyIndex + 6);
                }
                return details;

            }
            else if (message.Contains("StorageQueue Action -"))
            {
                PSJobHistoryStorageDetail details = new PSJobHistoryStorageDetail { ActionType = "Storage" };
                if (message.Contains("does not exist"))
                {
                    int firstIndex = message.IndexOf("'");
                    int secondIndex = message.IndexOf("'", firstIndex + 1);
                    details.StorageAccountName = "";
                    details.StorageQueueName = message.Substring(firstIndex + 1, secondIndex - (firstIndex + 1));
                    details.ResponseBody = message;
                    details.ResponseStatus = "Failed";
                }
                else
                {
                    int firstIndex = message.IndexOf("'");
                    int secondIndex = message.IndexOf("'", firstIndex + 1);
                    int thirdIndex = message.IndexOf("'", secondIndex + 1);
                    int fourthIndex = message.IndexOf("'", thirdIndex + 1);
                    details.StorageAccountName = message.Substring(firstIndex + 1, secondIndex - (firstIndex + 1));
                    details.StorageQueueName = message.Substring(thirdIndex + 1, fourthIndex - (thirdIndex + 1));
                    details.ResponseStatus = message.Substring(fourthIndex + 2);
                    details.ResponseBody = message;
                }
                return details;
            }
            return detail;
        }
        #endregion

        #region Get Job Details
        public PSJobDetail GetJobDetail(string jobCollection, string job, string cloudService)
        {
            CloudServiceGetResponse csDetails = csmClient.CloudServices.Get(cloudService);
            foreach (CloudServiceGetResponse.Resource csRes in csDetails.Resources)
            {
                if (csRes.ResourceProviderNamespace.Equals(Constants.SchedulerRPNameProvider, StringComparison.OrdinalIgnoreCase) && csRes.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase))
                {
                    SchedulerClient schedClient = new SchedulerClient(csmClient.Credentials, cloudService, jobCollection);
                    JobListResponse jobs = schedClient.Jobs.List(new JobListParameters
                    {
                        Skip = 0,
                    });
                    foreach (Job j in jobs)
                    {
                        if (j.Id.ToLower().Equals(job.ToLower()))
                        {
                            if(Enum.GetName(typeof(JobActionType), j.Action.Type).Contains("Http"))
                            {
                                return new PSHttpJobDetail
                                {
                                    JobName = j.Id,
                                    JobCollectionName = jobCollection,
                                    CloudService = cloudService,
                                    ActionType = Enum.GetName(typeof(JobActionType), j.Action.Type),
                                    Uri = j.Action.Request.Uri,
                                    Method = j.Action.Request.Method,
                                    Body = j.Action.Request.Body,
                                    Headers = j.Action.Request.Headers,
                                    Status = j.State.ToString(),
                                    StartTime = j.StartTime,
                                    EndSchedule = GetEndTime(j),
                                    Recurrence = j.Recurrence == null ? "" : j.Recurrence.Interval.ToString() + " per " + j.Recurrence.Frequency.ToString(),
                                    Failures = j.Status == null ? default(int?) : j.Status.FailureCount,
                                    Faults = j.Status == null ? default(int?) : j.Status.FaultedCount,
                                    Executions = j.Status == null ? default(int?) : j.Status.ExecutionCount,
                                    Lastrun = j.Status == null ? null : j.Status.LastExecutionTime,
                                    Nextrun = j.Status == null ? null : j.Status.NextExecutionTime
                                };
                            }
                            else
                            {
                                return new PSStorageQueueJobDetail
                                {
                                    JobName = j.Id,
                                    JobCollectionName = jobCollection,
                                    CloudService = cloudService,
                                    ActionType = Enum.GetName(typeof(JobActionType), j.Action.Type),
                                    StorageAccountName = j.Action.QueueMessage.StorageAccountName,
                                    StorageQueueName = j.Action.QueueMessage.QueueName,
                                    SasToken =  j.Action.QueueMessage.SasToken,
                                    QueueMessage = j.Action.QueueMessage.Message,
                                    Status = j.State.ToString(),
                                    EndSchedule = GetEndTime(j),
                                    StartTime = j.StartTime,
                                    Recurrence = j.Recurrence == null ? "" : j.Recurrence.Interval.ToString() + " per " + j.Recurrence.Frequency.ToString(),
                                    Failures = j.Status == null ? default(int?) : j.Status.FailureCount,
                                    Faults = j.Status == null ? default(int?) : j.Status.FaultedCount,
                                    Executions = j.Status == null ? default(int?) : j.Status.ExecutionCount,
                                    Lastrun = j.Status == null ? null : j.Status.LastExecutionTime,
                                    Nextrun = j.Status == null ? null : j.Status.NextExecutionTime
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region Delete Jobs

        public bool DeleteJob(string jobCollection, string jobName, string region = "")
        {
            if (!string.IsNullOrEmpty(region))
            {
                SchedulerClient schedulerClient = new SchedulerClient(csmClient.Credentials, region.ToCloudServiceName(), jobCollection);
                OperationResponse response = schedulerClient.Jobs.Delete(jobName);
                return response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
            }
            else if (string.IsNullOrEmpty(region))
            {
                CloudServiceListResponse csList = csmClient.CloudServices.List();
                foreach (CloudServiceListResponse.CloudService cs in csList.CloudServices)
                {
                    foreach (CloudServiceGetResponse.Resource csRes in csmClient.CloudServices.Get(cs.Name).Resources)
                    {
                        if (csRes.Type.Contains(Constants.JobCollectionResource))
                        {
                            JobCollectionGetResponse jcGetResponse = schedulerManagementClient.JobCollections.Get(cs.Name, csRes.Name);
                            if (jcGetResponse.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (PSSchedulerJob job in GetSchedulerJobs(cs.Name, jobCollection))
                                {
                                    if (job.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        SchedulerClient schedulerClient = new SchedulerClient(csmClient.Credentials, cs.Name, jobCollection);
                                        OperationResponse response = schedulerClient.Jobs.Delete(jobName);
                                        return response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region Delete Job Collection

        public bool DeleteJobCollection(string jobCollection, string region = "")
        {
            if (!string.IsNullOrEmpty(region))
            {
                SchedulerOperationStatusResponse response = schedulerManagementClient.JobCollections.Delete(region.ToCloudServiceName(), jobCollection);
                return response.StatusCode == System.Net.HttpStatusCode.OK ? true: false;
            }
            else if (string.IsNullOrEmpty(region))
            {
                CloudServiceListResponse csList = csmClient.CloudServices.List();
                foreach (CloudServiceListResponse.CloudService cs in csList.CloudServices)
                {
                    foreach (CloudServiceGetResponse.Resource csRes in csmClient.CloudServices.Get(cs.Name).Resources)
                    {
                        if (csRes.Type.Contains(Constants.JobCollectionResource))
                        {
                            JobCollectionGetResponse jcGetResponse = schedulerManagementClient.JobCollections.Get(cs.Name, csRes.Name);
                            if (jcGetResponse.Name.Equals(jobCollection, StringComparison.OrdinalIgnoreCase))
                            {
                                SchedulerOperationStatusResponse response = schedulerManagementClient.JobCollections.Delete(region.ToCloudServiceName(), jobCollection);
                                return response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        #endregion       
    }
        
}
