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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Endpoints
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;

    [Cmdlet(VerbsCommon.Set, "AzureAclConfig"), OutputType(typeof(NetworkAclObject))]
    public class SetAzureAclConfig : PSCmdlet
    {
        public const string AddRuleParameterSet = "AddRule";
        public const string RemoveRuleParameterSet = "RemoveRule";
        public const string SetRuleParameterSet = "SetRule";

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetAzureAclConfig.AddRuleParameterSet, HelpMessage = "Indicates that a rule should be added.")]
        public SwitchParameter AddRule { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetAzureAclConfig.RemoveRuleParameterSet, HelpMessage = "Indicates that a rule should be removed.")]
        public SwitchParameter RemoveRule { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Indicates that a rule should be set.")]
        public SwitchParameter SetRule { get; set; }

        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "ACL config.")]
        [ValidateNotNull]
        public NetworkAclObject ACL { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Rule ID to modify.")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetAzureAclConfig.RemoveRuleParameterSet, HelpMessage = "Rule ID to modify.")]
        public int RuleId { get; set; }
        
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = SetAzureAclConfig.AddRuleParameterSet, HelpMessage = "Allow or Deny rule.")]
        [Parameter(Mandatory = false, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Allow or Deny rule.")]
        [ValidateSet("Allow", "Deny", IgnoreCase=true)]
        public string Action { get; set; }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = SetAzureAclConfig.AddRuleParameterSet, HelpMessage = "Rule remote subnet.")]
        [Parameter(Mandatory = false, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Rule remote subnet.")]
        public string RemoteSubnet { get; set; }

        [Parameter(Position = 3, Mandatory = false, ParameterSetName = SetAzureAclConfig.AddRuleParameterSet, HelpMessage = "Order of processing for rule.")]
        [Parameter(Mandatory = false, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Order of processing for rule.")]
        public int? Order { get; set; }

        [Parameter(Position = 4, Mandatory = false, ParameterSetName = SetAzureAclConfig.AddRuleParameterSet, HelpMessage = "Rule description.")]
        [Parameter(Mandatory = false, ParameterSetName = SetAzureAclConfig.SetRuleParameterSet, HelpMessage = "Rule description.")]
        public string Description { get; set; }

        internal void ExecuteCommand()
        {
            switch(this.ParameterSetName)
            {
                case SetAzureAclConfig.AddRuleParameterSet:
                    this.ACL.AddRule(
                        new NetworkAclRule() {
                            Order = this.Order,
                            Action = this.Action,
                            RemoteSubnet = this.RemoteSubnet,
                            Description = this.Description
                        });

                    break;
                case SetAzureAclConfig.SetRuleParameterSet:
                    var rule = this.ACL.GetRule(this.RuleId);
                    if (rule != null)
                    {
                        // Check for parameters by name so we can tell the difference between 
                        // the user not specifying them, and the user specifying null/empty.
                        if(this.MyInvocation.BoundParameters.ContainsKey("Order"))
                        {
                            rule.Order = this.Order;
                        }

                        if (this.MyInvocation.BoundParameters.ContainsKey("Action"))
                        {
                            rule.Action = this.Action;
                        }

                        if (this.MyInvocation.BoundParameters.ContainsKey("RemoteSubnet"))
                        {
                            rule.RemoteSubnet = this.RemoteSubnet;
                        }

                        if (this.MyInvocation.BoundParameters.ContainsKey("Description"))
                        {
                            rule.Description = this.Description;
                        }
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("A rule with id '{0}' does not exist.", this.RuleId));
                    }

                    break;
                case SetAzureAclConfig.RemoveRuleParameterSet:
                    if (!this.ACL.RemoveRule(this.RuleId))
                    {
                        throw new ArgumentException(string.Format("A rule with id '{0}' does not exist.", this.RuleId));
                    }
                    break;
            }

            this.WriteObject(this.ACL);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                this.ExecuteCommand();
            }
            catch (Exception ex)
            {
                this.WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
