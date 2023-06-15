using AElf.Boilerplate.TestBase.Contract;
using AElf.Boilerplate.TestBase.DAppContract;
using AElf.Cryptography.ECDSA;
using AElf.CSharp.Core;

namespace AElf.Contracts.Timelock
{
    public class Module : ContractTestModule<TimelockContract>
    {
        
    }
    public class TestBase : DAppContractTestBase<Module>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        protected TStub GetContractStub<TStub>(ECKeyPair senderKeyPair) where TStub:ContractStubBase, new()
        {
            return GetTester<TStub>(DAppContractAddress, senderKeyPair);
        }
    }
}