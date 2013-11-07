

namespace WindowsAzurePowerShell.Test.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ValidatePathAttribute : ValidateEnumeratedArgumentsAttribute
    {
        protected override void ValidateElement(object element)
        {
            string path = element.ToString();

            if (!File.Exists(path))
            {
                throw new Exception("The provided path doesn't exist!");
            }
        }
    }
}
