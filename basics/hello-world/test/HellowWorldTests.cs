using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorldTests : TestBase
    {
        
        [Fact]
        public async Task Update_ShouldUpdateMessageAndFireEvent()
        {
            // Arrange
            var inputValue = "Hello, World!";
            var input = new StringValue { Value = inputValue };

            // Act
            await HellowWorldStub.Update.SendAsync(input);

            // Assert
            var updatedMessage = await HellowWorldStub.Read.CallAsync(new Empty());
            updatedMessage.Value.ShouldBe(inputValue);
        }

        [Fact]
        public async Task Read_ShouldReturnValue()
        {

            // Arrange
            var messageValue = "Hello, World!";
            var message = new StringValue { Value = messageValue };
            await HellowWorldStub.Update.SendAsync(message);

            //State.Message.Value = messageValue;
            var input = new Empty();

            // Act
            var result = await HellowWorldStub.Read.CallAsync(input);

            // Assert
            result.Value.ShouldBe(messageValue);
        }
        
        [Fact]
        public async Task PlayTests()
        {
            var message = new StringValue();
            message.Value = "Hello World";
            await HellowWorldStub.Update.SendAsync(message);

            (await HellowWorldStub.Read.CallAsync(new Empty())).Value.ShouldBe("Hello World");
        }
    }
    
}