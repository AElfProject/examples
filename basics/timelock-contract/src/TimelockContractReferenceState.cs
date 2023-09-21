using AElf.Standards.ACS0;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractState 
    {
        internal ACS0Container.ACS0ReferenceState GenesisContract { get; set; }
    }
}