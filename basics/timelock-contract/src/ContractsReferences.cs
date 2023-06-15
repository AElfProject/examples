using AElf.Contracts.MultiToken;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractState
    {
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
    }
}