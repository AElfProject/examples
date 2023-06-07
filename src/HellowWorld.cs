using Google.Protobuf.WellKnownTypes;
using System.Linq;
using AElf;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorld : HellowWorldContainer.HellowWorldBase
    {
        public override Empty Update(StringValue input)
        {
            State.Message.Value = input.Value;
            Context.Fire(new UpdatedMessage
            {
                Value = input.Value
            });
            return new Empty();
        }

        public override StringValue Read(Empty input)
        {
            StringValue value = new StringValue();
            value.Value = State.Message.Value;
            return value;
        }
    }
}