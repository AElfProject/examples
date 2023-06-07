using AElf.Contracts.MultiToken;
using AElf.Cryptography.ECDSA;
using AElf.Kernel.Token;
using AElf.Types;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorldTests
    {
        // private readonly ECKeyPair KeyPair;
        private readonly HellowWorldContainer.HellowWorldStub HellowWorldStub;
        private readonly TokenContractContainer.TokenContractStub TokenContractStub;
        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected ECKeyPair UserKeyPair => Accounts[1].KeyPair;
        protected Address UserAddress => Accounts[1].Address;

        public HellowWorldTests()
        {
            // KeyPair = SampleAccount.Accounts.First().KeyPair;
            HellowWorldStub = GetContractStub<HellowWorldContainer.HellowWorldStub>(DefaultKeyPair);
            TokenContractStub = GetTester<TokenContractContainer.TokenContractStub>(
                GetAddress(TokenSmartContractAddressNameProvider.StringName), DefaultKeyPair);
        }
    }
    
}