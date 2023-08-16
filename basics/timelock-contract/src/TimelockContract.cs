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
            if (State.Initialized.Value)
            {
                return new Empty();
            }
            State.Admin.Value = Context.Sender;
            State.Delay.Value = input.Delay;
            State.Initialized.Value = true;
            return new Empty();
        }

        public override Empty ChangeAdmin(ChangeAdminInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Assert(input.Admin != null, "NewAdmin must not be null");
            State.Admin.Value = input.Admin;
            Context.Fire(new NewAdmin
            {
                Admin = State.Admin.Value
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
                TxnHash = txnHash,
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
            State.TransactionQueue.Remove(txnHash);
            Context.Fire(new CancelTransaction
            {
                TxnHash = txnHash,
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
                TxnHash = txnHash,
                Target = input.Target,
                Method = input.Method,
                Data = input.Data,
                ExecuteTime = input.ExecuteTime
            });
            State.TransactionQueue.Remove(txnHash);
            return new Empty();
        }
    }
    
}