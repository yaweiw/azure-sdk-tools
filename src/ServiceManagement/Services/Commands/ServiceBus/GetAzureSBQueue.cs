namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using Commands.Utilities.Common;
    using Commands.Utilities.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Creates new service bus authorization rule.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSBQueue"), OutputType(typeof(QueueDescription), typeof(List<QueueDescription>))]
    public class GetAzureSBQueueCommand : CmdletWithSubscriptionBase
    {
        public const string NamespaceSASParameterSet = "NamespaceSAS";
        internal ServiceBusClientExtensions Client { get; set; }
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = NamespaceSASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The namespace name")]
        public string NameSpaceName;
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = NamespaceSASParameterSet, ValueFromPipelineByPropertyName = true, HelpMessage = "The filter")]
        public string Filter;
        public override void ExecuteCmdlet()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Client = Client ?? new ServiceBusClientExtensions(CurrentSubscription);
            IList<QueueDescription> descriptions = Client.GetQueues(NameSpaceName,Filter);
            WriteObject(descriptions, true);
            timer.Stop();
            WriteVerbose("Time Elapsed: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", timer.Elapsed.Hours, timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds / 10));
        }
    }
}
