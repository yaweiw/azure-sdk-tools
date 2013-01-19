// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.PowershellCore
{
    internal class PowershellCmdlet : PowershellEnvironment
    {
        private readonly CmdletsInfo cmdlet;

        public string Name
        {
            get
            {
                return cmdlet.name;
            }
        }

        public List<CmdletParam> Params
        {
            get
            {
                return cmdlet.parameters;
            }
        }

        public PowershellCmdlet(CmdletsInfo cmdlet, params PowershellModule[] modules) : base(modules)
        {
            this.cmdlet = cmdlet;
        }

        public PowershellCmdlet(CmdletsInfo cmdlet) : base()
        {
            this.cmdlet = cmdlet;
        }

        public override Collection<PSObject> Run()
        {
            Collection<PSObject> result = null;
            runspace.Open();
            using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
            {
                powershell.Runspace = runspace;
                powershell.AddCommand(cmdlet.name);
                if (cmdlet.parameters.Count > 0)
                {
                    foreach (var cmdletparam in cmdlet.parameters)
                    {
                        if(cmdletparam.value == null)
                        {
                            powershell.AddParameter(cmdletparam.name);
                        }
                        else
                        {
                            powershell.AddParameter(cmdletparam.name, cmdletparam.value);
                        }
                    }
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
            runspace.Close();

            return result;
        }
    }
}
