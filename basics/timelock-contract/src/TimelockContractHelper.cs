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

        public override Address GetAdmin(Empty input)
        {
            return State.Admin.Value;
        }

        public override BoolValue GetTransaction(Hash input)
        {
            return new BoolValue
            {
                Value = State.TransactionQueue[input]
            };
        }
    }
    
}