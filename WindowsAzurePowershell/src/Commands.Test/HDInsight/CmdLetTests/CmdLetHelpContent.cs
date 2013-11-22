using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Test.HDInsight.CmdLetTests
{
    public enum PipelineValue
    {
        False,
        TrueByValue,
        TrueByName
    }

    public class CmdletParameterValue
    {
        public bool Required { get; set; }
        public bool VariableLength { get; set; }
        public string DataType { get; set; }
    }

    public class CmdletParameter : CmdletParameterValue
    {
        public string Name { get; set; }
        public int Location { get; set; }
        public bool ValueFromPipeline { get; set; }
        public string Description { get; set; }
        public CmdletParameterValue Value { get; set; }
    }

    public class CmdletSyntax
    {
        public CmdletSyntax()
        {
            this.Parameters = new List<CmdletParameter>();
        }
        public string Name { get; set; }
        public IEnumerable<CmdletParameter> Parameters { get; private set; }
    }

    public class CmdletHelpContent
    {
        public CmdletHelpContent()
        {
            this.Syntax = new List<CmdletSyntax>();
            this.Parameters = new List<CmdletParameter>();
        }
        public string Name { get; set; }
        public string Verb { get; set; }
        public string Nown { get; set; }
        public string Description { get; set; }
        public IEnumerable<CmdletSyntax> Syntax { get; private set; }
        public IEnumerable<CmdletParameter> Parameters { get; private set; }
    }
}
