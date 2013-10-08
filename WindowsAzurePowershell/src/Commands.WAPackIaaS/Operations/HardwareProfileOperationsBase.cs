namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class HardwareProfileOperationsBase : OperationsBase<HardwareProfile>
    {
        public override HardwareProfile Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<HardwareProfile> Read()
        {
            throw new NotImplementedException();
        }
    }
}
