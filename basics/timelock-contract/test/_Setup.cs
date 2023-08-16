using AElf.Contracts.MultiToken;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AElf.Contracts.Timelock
{
    // This class is used to load the context required for unit testing.
    public class Module : Testing.TestBase.ContractTestModule<TimelockContract>
    {
        
    }
    
    // The TestBase class inherit ContractTestBase class, which is used to define and get stub classes required for unit testing.
    public class TestBase : Testing.TestBase.ContractTestBase<Module>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address ContractAddress => GetAddress(SmartContractAddressNameProvider.StringName);
        // Using the address and key to get stub, Like this:
        // TokenContractContainer.TokenContractStub stub = GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, keyPair);

        // private readonly ECKeyPair KeyPair;
        internal TimelockContractContainer.TimelockContractStub TimelockContractStub;
        internal TokenContractContainer.TokenContractStub TokenContractStub;

        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected Address UserAddress => Accounts[1].Address;
        protected IBlockTimeProvider BlockTimeProvider =>
            Application.ServiceProvider.GetRequiredService<IBlockTimeProvider>();

        public TestBase()
        {
            // KeyPair = SampleAccount.Accounts.First().KeyPair;
            TimelockContractStub = GetTimelockContractStub(DefaultKeyPair);
            TokenContractStub = GetTokenContractStub(DefaultKeyPair);
        }
        
        private TimelockContractContainer.TimelockContractStub GetTimelockContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TimelockContractContainer.TimelockContractStub>(ContractAddress, senderKeyPair);
        }
        
        private TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, senderKeyPair);
        }
    }
    
}