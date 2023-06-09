using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractState : ContractState 
    {
        public SingletonState<Address> Admin { get; set; }
        public SingletonState<Address> PendingAdmin { get; set; }
        public SingletonState<long> Delay { get; set; }
        public MappedState<Hash, Address> TransactionQueue { get; set; }
    }
}