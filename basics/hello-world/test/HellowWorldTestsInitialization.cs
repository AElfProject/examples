using AElf.Cryptography.ECDSA;
using AElf.Types;

namespace AElf.Contracts.HelloWorld
{
    public partial class HelloWorldTests
    {
        // private readonly ECKeyPair KeyPair;
        private readonly HelloWorldContainer.HelloWorldStub HelloWorldStub;
        protected ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        protected ECKeyPair UserKeyPair => Accounts[1].KeyPair;
        protected Address UserAddress => Accounts[1].Address;

        public HelloWorldTests()
        {
            HelloWorldStub = GetContractStub<HelloWorldContainer.HelloWorldStub>(DefaultKeyPair);
        }
    }
    
}