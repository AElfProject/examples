using AElf.Contracts.MultiToken;
using AElf.Cryptography.ECDSA;
using AElf.Kernel.Token;
using AElf.Types;
using Xunit.Abstractions;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        // private readonly ECKeyPair KeyPair;
        internal TimelockContractContainer.TimelockContractStub TimelockContractStub;
        internal TokenContractContainer.TokenContractStub TokenContractStub;

        protected Address TokenContractAddress;
        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected ECKeyPair UserKeyPair => Accounts[1].KeyPair;
        protected Address UserAddress => Accounts[1].Address;

        public TimelockContractTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            // KeyPair = SampleAccount.Accounts.First().KeyPair;
            TimelockContractStub = GetContractStub<TimelockContractContainer.TimelockContractStub>(DefaultKeyPair);
            TokenContractAddress = GetAddress(TokenSmartContractAddressNameProvider.StringName);
            TokenContractStub = GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, DefaultKeyPair);
        }
    }
    
}