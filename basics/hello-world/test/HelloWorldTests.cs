using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.HelloWorld
{
    public partial class HelloWorldTests : TestBase
    {
        [Fact]
        public async Task Update_ShouldUpdateMessageAndFireEvent()
        {
            // Arrange
            var inputValue = "Hello, World!";
            var input = new StringValue { Value = inputValue };

            // Act
            await HelloWorldStub.Update.SendAsync(input);

            // Assert
            var updatedMessage = await HelloWorldStub.Read.CallAsync(new Empty());
            updatedMessage.Value.ShouldBe(inputValue);
        }

        [Fact]
        public async Task Read_ShouldReturnValue()
        {
            // Arrange
            var messageValue = "Hello, World!";
            var message = new StringValue { Value = messageValue };
            await HelloWorldStub.Update.SendAsync(message);

            //State.Message.Value = messageValue;
            var input = new Empty();

            // Act
            var result = await HelloWorldStub.Read.CallAsync(input);

            // Assert
            result.Value.ShouldBe(messageValue);
        }
        
        [Fact]
        public async Task PlayTests()
        {
            var message = new StringValue
            {
                Value = "Hello World"
            };
            await HelloWorldStub.Update.SendAsync(message);

            var result = await HelloWorldStub.Read.CallAsync(new Empty());
            result.Value.ShouldBe("Hello World");
        }
    }
    
}