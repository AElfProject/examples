using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContract : TimelockContractContainer.TimelockContractBase
    {
        public override Empty Initialize(Empty input)
        {
            if (State.Initialized.Value)
            {
                return new Empty();
            }
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.Admin.Value = Context.Sender;
            State.Initialized.Value = true;
            return new Empty();
        }
        
        public override Empty SetDelay(DelayInput input)
        {
            Assert(State.Admin.Value == Context.Sender, "No permission");
            Assert(input != null, "Delay must not be null");
            Assert(input.Delay >= TimelockContractConstants.MIN_DELAY, "Delay must exceed minimum delay");
            Assert(input.Delay <= TimelockContractConstants.MAX_DELAY, "Delay must exceed maximum delay");
            State.Delay.Value = (long) input.Delay;

            Context.Fire(new NewDelay
            {
                NewDelay_ = input.Delay
            });
            return new Empty();
        }

        public override Empty AcceptAdmin(Empty input)
        {
            // Assert(Context.Sender == State.PendingAdmin.Value, "No permission");
            Assert(State.Admin.Value == Context.Sender, "No permission");
            Assert(State.PendingAdmin.Value != null, "PendingAdmin must not be null");
            State.Admin.Value = State.PendingAdmin.Value;
            Context.Fire(new NewAdmin
            {
                NewAdmin_ = State.Admin.Value
            });
            return new Empty();
        }

        public override Empty SetPendingAdmin(SetPendingAdminInput input)
        {
            // Assert(Context.Sender == Context.Self, "No permission");
            Assert(State.Admin.Value == Context.Sender, "No permission");
            State.PendingAdmin.Value = input.PendingAdmin;
            Context.Fire(new NewPendingAdmin
            {
                NewPendingAdmin_ = input.PendingAdmin
            });
            return new Empty();
        }

        public override Empty QueueTransaction(TransactionInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Assert(input.Eta >= Context.CurrentBlockTime.AddDays(State.Delay.Value), "Estimated execution block must satisfy delay");
            Hash txnHash = HashHelper.ComputeFrom(input);
            State.TransactionQueue[txnHash] = Context.Sender;
            Context.Fire(new QueueTransaction
            {
                TxnHash = txnHash,
                Target = input.Target,
                Amount = input.Amount,
                Signature = input.Signature,
                Data = input.Data,
                Eta = input.Eta
            });
            return new Empty();
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
                Amount = input.Amount,
                Signature = input.Signature,
                Data = input.Data,
                Eta = input.Eta
            });
            return new Empty();
        }

        public override Empty ExecuteTransaction(TransactionInput input)
        {
            Assert(Context.Sender == State.Admin.Value, "No permission");
            Hash txnHash = HashHelper.ComputeFrom(input);
            Assert(State.TransactionQueue[txnHash] != null, "executeTransaction: Transaction hasn't been queued");
            Assert(Context.CurrentBlockTime >= input.Eta, "executeTransaction: Transaction hasn't surpassed time lock");
            Assert(Context.CurrentBlockTime <= input.Eta.AddDays(TimelockContractConstants.GRACE_PERIOD), "executeTransaction: Transaction is stale");

            Address from = State.TransactionQueue[txnHash];
            State.TokenContract.Value = Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                From = from,
                To = input.Target,
                Amount = (long) input.Amount,
                Symbol = TimelockContractConstants.SYMBOL,
                Memo = "Execution"
            });
            Context.Fire(new ExecuteTransaction
            {
                TxnHash = txnHash,
                Target = input.Target,
                Amount = input.Amount,
                Signature = input.Signature,
                Data = input.Data,
                Eta = input.Eta
            });
            State.TransactionQueue.Remove(txnHash);
            return new Empty();
        }
    }
    
}