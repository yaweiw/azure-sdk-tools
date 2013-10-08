namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class CloudOperationsBase : OperationsBase<Cloud>
    {
        public override Cloud Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<Cloud> Read()
        {
            throw new NotImplementedException();
        }
    }
}
