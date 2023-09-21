using System.IO;
using AElf.Contracts.MultiToken;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Kernel;
using AElf.Kernel.SmartContract;
using AElf.Standards.ACS0;
using AElf.Types;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace AElf.Contracts.Timelock
{
    // This class is used to load the context required for unit testing.
    public class Module : Testing.TestBase.ContractTestModule<TimelockContract>
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IBlockTimeProvider, BlockTimeProvider>();
            Configure<ContractOptions>(o => o.ContractDeploymentAuthorityRequired = false);
        }
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
        internal ACS0Container.ACS0Stub ZeroContractStub;

        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected Address UserAddress => Accounts[1].Address;
        protected Address TimelockContractAddress;
        protected IBlockTimeProvider BlockTimeProvider =>
            Application.ServiceProvider.GetRequiredService<IBlockTimeProvider>();

        public TestBase()
        {
            ZeroContractStub = GetContractZeroTester(DefaultKeyPair);
            var result = AsyncHelper.RunSync(async () =>await ZeroContractStub.DeploySmartContract.SendAsync(new ContractDeploymentInput
            {   
                Category = KernelConstants.CodeCoverageRunnerCategory,
                Code = ByteString.CopyFrom(
                    File.ReadAllBytes(typeof(TimelockContract).Assembly.Location))
            }));

            TimelockContractAddress = Address.Parser.ParseFrom(result.TransactionResult.ReturnValue);
            // KeyPair = SampleAccount.Accounts.First().KeyPair;
            TimelockContractStub = GetTimelockContractStub(DefaultKeyPair);
            TokenContractStub = GetTokenContractStub(DefaultKeyPair);
        }
        
        private TimelockContractContainer.TimelockContractStub GetTimelockContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TimelockContractContainer.TimelockContractStub>(TimelockContractAddress, senderKeyPair);
        }
        
        private TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, senderKeyPair);
        }
        
        private ACS0Container.ACS0Stub GetContractZeroTester(ECKeyPair keyPair)
        {
            return GetTester<ACS0Container.ACS0Stub>(BasicContractZeroAddress, keyPair);
        }
    }
    
}