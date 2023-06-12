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
        //Method to update the message value. 
        public override Empty Update(StringValue input)
        {
            // Set the message value in the contract state
            State.Message.Value = input.Value;
            // Trigger an event to notify listeners about the message update
            Context.Fire(new UpdatedMessage
            {
                Value = input.Value
            });
            // Return an empty response
            return new Empty();
        }

        // Method to read the current message value
        public override StringValue Read(Empty input)
        {
            // Create a new StringValue object to hold the message value
            StringValue value = new StringValue();
            // Set the value from the contract state
            value.Value = State.Message.Value;
            // Return the StringValue object
            return value;
        }
    }
}