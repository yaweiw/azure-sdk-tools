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

using System.Linq;
using System.Threading; //

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.PowershellCore
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public class PowershellCmdletScript : PowershellEnvironment
    {
        private readonly List<string> cmdlets;
        
        //public string Name
        //{
        //    get
        //    {
        //        return cmdlet.name;
        //    }
        //}

        //public List<CmdletParam> Params
        //{
        //    get
        //    {
        //        return cmdlet.parameters;
        //    }
        //}

        public PowershellCmdletScript(List<string> cmdlet, params PowershellModule[] modules) : base(modules)
        {
            this.cmdlets = cmdlet;            
        }
        
       

        public PowershellCmdletScript(List<string> cmdlet) : base()
        {
            this.cmdlets = cmdlet;
        }

        public PowershellCmdletScript(PowershellModule[] modules) : base(modules)
        {
            this.cmdlets = new List<string>();
        }
         
        public PowershellCmdletScript() : base()
        {
            this.cmdlets = new List<string>();
        }


        public void Add(string cmdlet)
        {
            this.cmdlets.Add(cmdlet);
        }
        public override Collection<PSObject> Run()
        {
            Collection<PSObject> result = null;
            runspace.Open();
            for (int i = 0; i < cmdlets.Count; i++)
            {

                using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
                {
                    powershell.Runspace = runspace;

                    if (!String.IsNullOrWhiteSpace(cmdlets[i]))
                    {
                        powershell.AddScript(cmdlets[i]);
                    }
                    result = powershell.Invoke();

                    if (powershell.Streams.Error.Count > 0)
                    {
                        runspace.Close();

                        List<Exception> exceptions = new List<Exception>();
                        foreach (ErrorRecord error in powershell.Streams.Error)
                        {
                            exceptions.Add(new Exception(error.Exception.Message));
                        }

                        throw new AggregateException(exceptions);
                    }
                }
            }
            runspace.Close();

            return result;
        }

        //public PSInvocationState RunAndStop(int ms)
        //{
        //    PSInvocationState result = 0;
        //    runspace.Open();
        //    using (var powershell = PowerShell.Create())
        //    {
        //        powershell.Runspace = runspace;
        //        powershell.AddCommand(cmdlet.name);
        //        if (cmdlet.parameters.Count > 0)
        //        {
        //            foreach (var cmdletparam in cmdlet.parameters)
        //            {
        //                if (cmdletparam.value == null)
        //                {
        //                    powershell.AddParameter(cmdletparam.name);
        //                }
        //                else
        //                {
        //                    powershell.AddParameter(cmdletparam.name, cmdletparam.value);
        //                }
        //            }
        //        }

        //        powershell.BeginInvoke();
        //        Thread.Sleep(ms);
        //        powershell.Stop();

        //        result = powershell.InvocationStateInfo.State;
        //    }
        //    runspace.Close();

        //    return result;
        //}
    }
}
