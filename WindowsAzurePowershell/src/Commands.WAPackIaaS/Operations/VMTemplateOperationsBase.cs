namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class VMTemplateOperationsBase : OperationsBase<VMTemplate>
    {
        public override VMTemplate Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<VMTemplate> Read()
        {
            throw new NotImplementedException();
        }
    }
}
