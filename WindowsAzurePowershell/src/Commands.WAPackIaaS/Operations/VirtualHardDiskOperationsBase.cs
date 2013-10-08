namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class VirtualHardDiskOperationsBase : OperationsBase<VirtualHardDisk>
    {
        public override VirtualHardDisk Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<VirtualHardDisk> Read()
        {
            throw new NotImplementedException();
        }
    }
}
