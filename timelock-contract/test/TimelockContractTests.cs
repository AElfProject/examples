using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractTests : TestBase
    {
        [Fact]
        public async Task InitializeTests()
        {
            await TimelockContractStub.Initialize.SendAsync(new Empty());
        }
        
        [Fact]
        public async Task SetDelayTests()
        {
            await InitializeTests();
            DelayInput input = new DelayInput();
            input.Delay = 3L;
            await TimelockContractStub.SetDelay.SendAsync(input);
            var result = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            result.Value.ShouldBe(3L);
        }

        [Fact]
        public async Task AcceptAdminTests()
        {
            await InitializeTests();
            SetPendingAdminInput input = new SetPendingAdminInput();
            input.PendingAdmin = UserAddress;
            await TimelockContractStub.SetPendingAdmin.SendAsync(input);
            await Task.Delay(2000);
            await TimelockContractStub.AcceptAdmin.SendAsync(new Empty());
            await Task.Delay(2000);
            var result = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            result.ShouldBe(UserAddress);
        }

    }
    
}