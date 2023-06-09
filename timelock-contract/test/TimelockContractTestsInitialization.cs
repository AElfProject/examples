using AElf.Contracts.MultiToken;
using AElf.Cryptography.ECDSA;
using AElf.Kernel.Token;
using AElf.Types;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractTests
    {
        // private readonly ECKeyPair KeyPair;
        private readonly TimelockContractContainer.TimelockContractStub TimelockContractStub;
        private readonly TokenContractContainer.TokenContractStub TokenContractStub;
        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected ECKeyPair UserKeyPair => Accounts[1].KeyPair;
        protected Address UserAddress => Accounts[1].Address;

        public TimelockContractTests()
        {
            // KeyPair = SampleAccount.Accounts.First().KeyPair;
            TimelockContractStub = GetContractStub<TimelockContractContainer.TimelockContractStub>(DefaultKeyPair);
            TokenContractStub = GetTester<TokenContractContainer.TokenContractStub>(
                GetAddress(TokenSmartContractAddressNameProvider.StringName), DefaultKeyPair);
        }
    }
    
}