using AElf.Contracts.Consensus.AEDPoS;
using AElf.Contracts.MultiToken;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorldState
    {
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
        internal AEDPoSContractContainer.AEDPoSContractReferenceState ConsensusContract { get; set; }
    }
}