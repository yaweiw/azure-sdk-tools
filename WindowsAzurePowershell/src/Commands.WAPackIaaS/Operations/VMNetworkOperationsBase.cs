namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class VMNetworkOperationsBase : OperationsBase<VMNetwork>
    {
        public override VMNetwork Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<VMNetwork> Read()
        {
            throw new NotImplementedException();
        }
    }
}
