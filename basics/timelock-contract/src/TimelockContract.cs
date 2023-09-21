using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContract : TimelockContractContainer.TimelockContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            Assert(!State.Initialized.Value, "Already initialized.");
            State.GenesisContract.Value = Context.GetZeroSmartContractAddress();
            var author = State.GenesisContract.GetContractAuthor.Call(Context.Self);
            Assert(author == Context.Sender, "No permission.");
            Assert(input.Delay <= TimelockContractConstants.MAX_DELAY, "Delay must not exceed maximum delay");
            Assert(input.Delay >= TimelockContractConstants.MIN_DELAY, "Delay must exceed minimum delay");
            State.Admin.Value = Context.Sender;
            State.Delay.Value = input.Delay;
            State.Initialized.Value = true;
            return new Empty();
        }

        public override Empty ChangeAdmin(ChangeAdminInput input)
        {
            Assert(Context.Sender == Context.Self, "No permission");
            Assert(input.Admin != null, "NewAdmin must not be null");
            State.Admin.Value = input.Admin;
            Context.Fire(new NewAdmin
            {
                Admin = State.Admin.Value
            });
            return new Empty();
        }

        public override Empty SetDelay(SetDelayInput input)
        {
            Assert(Context.Sender == Context.Self, "No permission");
            Assert(input.Delay <= TimelockContractConstants.MAX_DELAY, "Delay must not exceed maximum delay");
            Assert(input.Delay >= TimelockContractConstants.MIN_DELAY, "Delay must exceed minimum delay");
            State.Delay.Value = input.Delay;
            Context.Fire(new NewDelay
            {
                Delay = State.Delay.Value
            });
            return new Empty();
        }

        public override Hash QueueTransaction(TransactionInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Assert(input.ExecuteTime >= Context.CurrentBlockTime.AddSeconds((long) State.Delay.Value), "Estimated execution block must satisfy delay");
            Hash txnHash = HashHelper.ComputeFrom(input);
            State.TransactionQueue[txnHash] = true;
            Context.Fire(new QueueTransaction
            {
                Target = input.Target,
                Method = input.Method,
                Data = input.Data,
                ExecuteTime = input.ExecuteTime
            });
            return txnHash;
        }

        public override Empty CancelTransaction(TransactionInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Hash txnHash = HashHelper.ComputeFrom(input);
            State.TransactionQueue[txnHash] = false;
            Context.Fire(new CancelTransaction
            {
                Target = input.Target,
                Method = input.Method,
                Data = input.Data,
                ExecuteTime = input.ExecuteTime
            });
            return new Empty();
        }

        public override Empty ExecuteTransaction(TransactionInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Hash txnHash = HashHelper.ComputeFrom(input);
            Assert(State.TransactionQueue[txnHash], "executeTransaction: Transaction hasn't been queued");
            Assert(Context.CurrentBlockTime >= input.ExecuteTime, "executeTransaction: Transaction hasn't surpassed time lock");
            Assert(Context.CurrentBlockTime <= input.ExecuteTime.AddSeconds(TimelockContractConstants.GRACE_PERIOD), "executeTransaction: Transaction is stale");
            
            State.TransactionQueue[txnHash] = false;
            Context.SendInline(input.Target, input.Method, input.Data);
            Context.Fire(new ExecuteTransaction
            {
                Target = input.Target,
                Method = input.Method,
                Data = input.Data,
                ExecuteTime = input.ExecuteTime
            });
            return new Empty();
        }
    }
    
}