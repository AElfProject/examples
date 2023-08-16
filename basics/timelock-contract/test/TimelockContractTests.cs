using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
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
        private readonly Timestamp _currentTime = TimestampHelper.GetUtcNow();
        
        [Fact]
        public async Task InitializeTests()
        {
            InitializeInput input = new InitializeInput
            {
                Delay = 5
            };
            await TimelockContractStub.Initialize.SendAsync(input);
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
                ExecuteTime = _currentTime.AddSeconds(10)
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
        }

        [Fact]
        public async Task CancelTransactionTests()
        {
            await InitializeTests();
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
                ExecuteTime = _currentTime.AddSeconds(10)
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
                ExecuteTime = _currentTime.AddSeconds(10)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(10));
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