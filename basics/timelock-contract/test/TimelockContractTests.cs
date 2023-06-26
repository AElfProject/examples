using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Kernel;
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
            input.Delay = 2 * 24 * 60 * 60;
            await TimelockContractStub.SetDelay.SendAsync(input);
            var result = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            result.Value.ShouldBe(2 * 24 * 60 * 60);
        }

        [Fact]
        public async Task NewAdminTests()
        {
            await InitializeTests();
            ChangeAdminInput changeAdminInput = new ChangeAdminInput
            {
                Admin = UserAddress
            };
            await TimelockContractStub.ChangeAdmin.SendAsync(changeAdminInput);
            var result = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            result.ShouldBe(UserAddress);
        }
        
        [Fact]
        public async Task QueueTransactionTests()
        {
            await InitializeTests();
            TransferInput transferInput = new TransferInput
            {
                To = UserAddress,
                Symbol = "ELF",
                Amount = 100
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = UserAddress,
                Method = "Transfer",
                Data = transferInput.ToByteString(),
                Eta = TimestampHelper.GetUtcNow(),
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
            _testOutputHelper.WriteLine(txnHash.ToString());
        }

        [Fact]
        public async Task CancelTransactionTests()
        {
            await InitializeTests();
            TransferInput transferInput = new TransferInput
            {
                To = UserAddress,
                Symbol = "ELF",
                Amount = 100
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = UserAddress,
                Method = "Transfer",
                Data = transferInput.ToByteString(),
                Eta = TimestampHelper.GetUtcNow(),
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            await TimelockContractStub.CancelTransaction.SendAsync(transactionInput);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = false
            };
            result.ShouldBe(expectResult);
        }
        
        private async Task InitializeAsync()
        {
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = ContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = ContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
        }
        
        /**
         * Because of time lock limit, need to comment out the assert of the ExecuteTransaction method.
         */
        [Fact]
        public async Task ExecuteTransactionTests()
        {
            await InitializeTests();
            await InitializeAsync();
            var balance1 = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput 
            {
                Owner = DefaultAddress, 
                Symbol = "ELF"
            });
            _testOutputHelper.WriteLine(balance1.Balance.ToString());
            TransferInput transferInput = new TransferInput
            {
                To = UserAddress,
                Symbol = "ELF",
                Amount = 502
            };
            TransferFromInput transferFromInput = new TransferFromInput
            {
                From = DefaultAddress,
                To = UserAddress,
                Amount = 502,
                Symbol = "ELF",
                Memo = "TEST"
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = TokenContractAddress,
                Method = "TransferFrom",
                Data = transferFromInput.ToByteString(),
                Eta = TimestampHelper.GetUtcNow(),
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            
            var balance2 = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput 
            {
                Owner = UserAddress, 
                Symbol = "ELF"
            });
            var balance3 = await TokenContractStub.GetBalance.CallAsync(new GetBalanceInput 
            {
                Owner = DefaultAddress, 
                Symbol = "ELF"
            });
            balance2.Balance.ShouldBe(502);
            balance3.Balance.ShouldBe(balance1.Balance.Sub(502));
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash.Output);
            var expectResult = new BoolValue
            {
                Value = false
            };
            result.ShouldBe(expectResult);
        }
        
    }
    
}