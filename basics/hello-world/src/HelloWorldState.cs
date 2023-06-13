using AElf.Sdk.CSharp.State;

namespace AElf.Contracts.HelloWorld
{
    public partial class HelloWorldState : ContractState 
    {
        public StringState Message { get; set; }
    }
}