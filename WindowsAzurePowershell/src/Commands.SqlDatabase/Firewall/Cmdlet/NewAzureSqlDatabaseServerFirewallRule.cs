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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Firewall.Cmdlet
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using Model;
    using Properties;
    using ServiceManagement;
    using Services;

    /// <summary>
    /// Creates a new firewall rule for a Windows Azure SQL Database server in the selected subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabaseServerFirewallRule", 
        DefaultParameterSetName = IpRangeParameterSet, 
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    public class NewAzureSqlDatabaseServerFirewallRule : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// The default rule name for allowing all Azure services.  This is used when a
        /// rule name is not specified for the AllowAllAzureServicesParameterSet parameter
        /// set
        /// </summary>
        private const string AllowAllAzureServicesRuleName = "AllowAllAzureServices";

        #region Parameter Sets

        /// <summary>
        /// Parameter set that uses an IP Range
        /// </summary>
        internal const string IpRangeParameterSet = "IpRange";

        /// <summary>
        /// Parameter set for allowing all azure services
        /// </summary>
        internal const string AllowAllAzureServicesParameterSet = "AllowAllAzureServices";

        #endregion

        /// <summary>
        /// The special IP for the beginning and ending of the firewall rule that will
        /// allow all azure services to connect to the server.
        /// </summary>
        private const string AllowAzureServicesRuleAddress = "0.0.0.0";

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="NewAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        public NewAzureSqlDatabaseServerFirewallRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="NewAzureSqlDatabaseServerFirewallRule"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureSqlDatabaseServerFirewallRule(ISqlDatabaseManagement channel)
        {
            this.Channel = channel;
        }

        /// <summary>
        /// Gets or sets the name of the server to connect add the firewall rule to
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, 
            HelpMessage = "SQL Database server name.")]
        [ValidateNotNullOrEmpty]
        public string ServerName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the fire wall rule
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = IpRangeParameterSet,
            HelpMessage = "SQL Database server firewall rule name.")]
        [Parameter(Mandatory = false, ParameterSetName = AllowAllAzureServicesParameterSet,
            HelpMessage = "SQL Database server firewall rule name.")]
        [ValidateNotNullOrEmpty]
        public string RuleName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the starting IP address for the rule
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Start of the IP Range.", 
            ParameterSetName = IpRangeParameterSet)]
        [ValidateNotNullOrEmpty]
        public string StartIpAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ending IP address for the firewall rule
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "End of the IP Range.", 
            ParameterSetName = IpRangeParameterSet)]
        [ValidateNotNullOrEmpty]
        public string EndIpAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not to allow all windows azure services to connect
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Allow all Azure services access to the server.", 
            ParameterSetName = AllowAllAzureServicesParameterSet)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter AllowAllAzureServices 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets whether or not to force the operation to proceed.
        /// </summary>
        [Parameter(HelpMessage = "Do not confirm on the creation of the firewall rule")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new firewall rule on the specified server.
        /// </summary>
        /// <param name="parameterSetName">
        /// The parameter set for the command.
        /// </param>
        /// <param name="serverName">
        /// The name of the server in which to create the firewall rule.
        /// </param>
        /// <param name="ruleName">
        /// The name of the new firewall rule.
        /// </param>
        /// <param name="startIpAddress">
        /// The starting IP address for the firewall rule.
        /// </param>
        /// <param name="endIpAddress">
        /// The ending IP address for the firewall rule.
        /// </param>
        /// <returns>The context to the newly created firewall rule.</returns>
        internal SqlDatabaseServerFirewallRuleContext NewAzureSqlDatabaseServerFirewallRuleProcess(
            string parameterSetName, 
            string serverName, 
            string ruleName, 
            string startIpAddress, 
            string endIpAddress)
        {
            SqlDatabaseServerFirewallRuleContext operationContext = null;
            try
            {
                this.InvokeInOperationContext(() =>
                {
                    this.RetryCall(subscription =>
                        this.Channel.NewServerFirewallRule(
                            subscription, 
                            serverName, 
                            ruleName, 
                            startIpAddress, 
                            endIpAddress));

                    Operation operation = WaitForSqlDatabaseOperation();

                    operationContext = new SqlDatabaseServerFirewallRuleContext()
                    {
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus = operation.Status,
                        OperationId = operation.OperationTrackingId,
                        ServerName = serverName,
                        RuleName = ruleName,
                        StartIpAddress = startIpAddress,
                        EndIpAddress = endIpAddress
                    };
                });
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);
            }

            return operationContext;
        }

        /// <summary>
        /// Process the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Do nothing if force is not specified and user cancelled the operation
            string verboseDescription = string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.NewAzureSqlDatabaseServerFirewallRuleDescription,
                        this.RuleName,
                        this.ServerName);
            
            string verboseWarning = string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.NewAzureSqlDatabaseServerFirewallRuleWarning,
                        this.RuleName,
                        this.ServerName);

            if (!this.Force.IsPresent &&
                !this.ShouldProcess(verboseDescription, verboseWarning, Resources.ShouldProcessCaption))
            {
                return;
            }

            try
            {
                base.ProcessRecord();
                SqlDatabaseServerOperationContext context = null;

                switch (this.ParameterSetName)
                {
                    case IpRangeParameterSet:
                        context = this.NewAzureSqlDatabaseServerFirewallRuleProcess(
                            this.ParameterSetName,
                            this.ServerName,
                            this.RuleName,
                            this.StartIpAddress,
                            this.EndIpAddress);
                        break;

                    case AllowAllAzureServicesParameterSet:

                        //Determine which rule name to use.
                        string ruleName = AllowAllAzureServicesRuleName;
                        if (this.MyInvocation.BoundParameters.ContainsKey("RuleName"))
                        {
                            ruleName = this.RuleName;
                        }

                        //Create the rule
                        context = this.NewAzureSqlDatabaseServerFirewallRuleProcess(
                            this.ParameterSetName,
                            this.ServerName,
                            ruleName,
                            AllowAzureServicesRuleAddress,
                            AllowAzureServicesRuleAddress);
                        break;
                }

                if (context != null)
                {
                    this.WriteObject(context, true);
                }
            }
            catch (Exception ex)
            {
                this.WriteWindowsAzureError(new ErrorRecord(ex, string.Empty, ErrorCategory.WriteError, null));
            }
        }
    }
}
