using System;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractState : ContractState 
    {
        public BoolState Initialized { get; set; }
        public SingletonState<Address> Admin { get; set; }
        public SingletonState<long> Delay { get; set; }
        public MappedState<Hash, bool> TransactionQueue { get; set; }
    }
}