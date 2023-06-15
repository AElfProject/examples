using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContract
    {
        public override Int64Value GetDelay(Empty input)
        {
            var delay = new Int64Value
            {
                Value = State.Delay.Value
            };
            return delay;
        }

        public override Address GetPendingAdmin(Empty input)
        {
            return State.PendingAdmin.Value;
        }
        
        public override Address GetAdmin(Empty input)
        {
            return State.Admin.Value;
        }
        
        public override Address GetTransaction(Hash input)
        {
            return State.TransactionQueue[input];
        }
    }
    
}