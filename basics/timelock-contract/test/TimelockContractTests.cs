using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Kernel;
using AElf.Types;
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
            await TimelockContractStub.AcceptAdmin.SendAsync(new Empty());
            var result = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            result.ShouldBe(UserAddress);
        }
        
        [Fact]
        public async Task QueueTransactionTests()
        {
            await InitializeTests();
            TransactionInput input = new TransactionInput
            {
                Target = UserAddress,
                Amount = 3L,
                Data = "",
                Eta = TimestampHelper.GetUtcNow(),
                Signature = "sign"
            };
            Hash txnHash = HashHelper.ComputeFrom(input);
            await TimelockContractStub.QueueTransaction.SendAsync(input);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task CancelTransactionTests()
        {
            await InitializeTests();
            TransactionInput input = new TransactionInput
            {
                Target = UserAddress,
                Amount = 3L,
                Data = "",
                Eta = TimestampHelper.GetUtcNow(),
                Signature = "sign"
            };
            Hash txnHash = HashHelper.ComputeFrom(input);
            await TimelockContractStub.QueueTransaction.SendAsync(input);
            await TimelockContractStub.CancelTransaction.SendAsync(input);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            result.ShouldBe(new Address());
        }
        
        private async Task InitializeAsync()
        {
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = DAppContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
        }
        
        [Fact]
        public async Task ExecuteTransactionTests()
        {
            await InitializeTests();
            await InitializeAsync();
            TransactionInput input = new TransactionInput
            {
                Target = UserAddress,
                Amount = 5L,
                Data = "",
                Eta = TimestampHelper.GetUtcNow(),
                Signature = "sign"
            };
            await TimelockContractStub.QueueTransaction.SendAsync(input);
            await TimelockContractStub.ExecuteTransaction.SendAsync(input);
            
            var balance2 = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput 
            {
                Owner = UserAddress, 
                Symbol = "ELF"
            });
            balance2.Balance.ShouldBe(5L);
        }
        
    }
    
}