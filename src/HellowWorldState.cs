using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorldState : ContractState 
    {
        public StringState Message { get; set; }
    }
}