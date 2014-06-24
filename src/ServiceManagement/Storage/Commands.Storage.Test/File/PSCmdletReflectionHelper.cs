﻿// ----------------------------------------------------------------------------------
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

 namespace Microsoft.WindowsAzure.Management.Storage.Test.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Text;

    internal static class PSCmdletReflectionHelper
    {
        private static readonly Type psCmdletType = typeof(PSCmdlet);

        private static readonly FieldInfo parameterSetFieldInfo = typeof(System.Management.Automation.Cmdlet).GetField("_parameterSetName", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo beginProcessingMethodInfo = psCmdletType.GetMethod("BeginProcessing", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo endProcessingMethodInfo = psCmdletType.GetMethod("EndProcessing", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo processRecordMethodInfo = psCmdletType.GetMethod("ProcessRecord", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly object[] emptyParameters = new object[0];

        public static void RunCmdlet(this PSCmdlet cmdlet, string parameterSet, params KeyValuePair<string, object>[] parameters)
        {
            RunCmdlet(cmdlet, parameterSet,
                parameters.Select(x => new KeyValuePair<string, object[]>(
                    x.Key, new object[] { x.Value })).ToArray()
            );
        }

        public static void RunCmdlet(this PSCmdlet cmdlet, string parameterSet, KeyValuePair<string, object[]>[] incomingValues)
        {
            var cmdletType = cmdlet.GetType();
            parameterSetFieldInfo.SetValue(cmdlet, parameterSet);
            beginProcessingMethodInfo.Invoke(cmdlet, emptyParameters);
            var parameterProperties = incomingValues.Select(x =>
                new Tuple<PropertyInfo, object[]>(
                    cmdletType.GetProperty(x.Key),
                    x.Value)).ToArray();

            for (int i = 0; i < incomingValues[0].Value.Length; i++)
            {
                foreach (var parameter in parameterProperties)
                {
                    parameter.Item1.SetValue(cmdlet, parameter.Item2[i], null);
                }

                processRecordMethodInfo.Invoke(cmdlet, emptyParameters);
            }

            endProcessingMethodInfo.Invoke(cmdlet, emptyParameters);
        }
    }
}
