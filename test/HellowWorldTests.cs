using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace Com.Contracts.HellowWorld
{
    public partial class HellowWorldTests : TestBase
    {
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