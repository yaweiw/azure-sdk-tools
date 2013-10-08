namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Operations
{
    using DataContract;
    using System;
    using System.Collections.Generic;

    public class VirtualMachineOperationsBase : OperationsBase<VirtualMachine>
    {
        public override VirtualMachine Create(VirtualMachine toCreate, out Guid jobId)
        {
            throw new NotImplementedException();
        }

        public override VirtualMachine Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public override List<VirtualMachine> Read()
        {
            throw new NotImplementedException();
        }

        public List<VirtualMachine> Read(string vmName)
        {
            throw new NotImplementedException();
        }

        public override VirtualMachine Update(VirtualMachine toUpdate, out Guid jobId)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(Guid id, out Guid jobId)
        {
            throw new NotImplementedException();
        }
    }
}
