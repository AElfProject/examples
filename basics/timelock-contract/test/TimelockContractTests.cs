using System;
using System.Linq;
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
            ulong delay = 2 * 24 * 60 * 60;
            InitializeInput input = new InitializeInput
            {
                Delay = delay
            };
            await TimelockContractStub.Initialize.SendAsync(input);
            var getDelay = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            delay.ShouldBe(getDelay.Value);
        }
        
        [Fact]
        public async Task InitializeDuplicateTests()
        {
            InitializeInput input = new InitializeInput
            {
                Delay = 2 * 24 * 60 * 60
            };
            try
            {
                await TimelockContractStub.Initialize.SendAsync(input);
                await TimelockContractStub.Initialize.SendAsync(input);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Already initialized.");
            }
        }

        [Fact]
        public async Task ChangeAdminTests()
        {
            await InitializeTests();
            var address = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            address.ShouldBe(DefaultAddress);
            ChangeAdminInput changeAdminInput = new ChangeAdminInput()
            {
                Admin = UserAddress
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = TimelockContractAddress,
                Method = "ChangeAdmin",
                Data = changeAdminInput.ToByteString(),
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));
            var txRes = await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            
            var addressAfter = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            addressAfter.ShouldBe(UserAddress);
            
            var newAdminEvent = NewAdmin.Parser
                .ParseFrom(txRes.TransactionResult.Logs.First(l => l.Name.Contains(nameof(NewAdmin)))
                    .NonIndexed);
            newAdminEvent.Admin.ShouldBe(changeAdminInput.Admin);
        }
        
        [Fact]
        public async Task ChangeAdminTests_NoPermission()
        {
            await InitializeTests();
            ChangeAdminInput changeAdminInput = new ChangeAdminInput()
            {
                Admin = UserAddress
            };
            try
            {
                await TimelockContractStub.ChangeAdmin.SendAsync(changeAdminInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("No permission");
            }
            var addressAfter = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            addressAfter.ShouldBe(DefaultAddress);
        }
        
        [Fact]
        public async Task ChangeAdminTests_InputNull()
        {
            await InitializeTests();
            var address = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            address.ShouldBe(DefaultAddress);
            ChangeAdminInput changeAdminInput = new ChangeAdminInput()
            {
                Admin = null
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = TimelockContractAddress,
                Method = "ChangeAdmin",
                Data = changeAdminInput.ToByteString(),
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));

            try
            {
                await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("NewAdmin must not be null");
            }
            var addressAfter = await TimelockContractStub.GetAdmin.CallAsync(new Empty());
            addressAfter.ShouldBe(DefaultAddress);
        }
        
        [Fact]
        public async Task SetDelayTests()
        {
            await InitializeTests();
            UInt64Value delay = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            delay.Value.ShouldBe((uint)(2 * 24 * 60 * 60));
            SetDelayInput setDelayInput = new SetDelayInput
            {
                Delay = 4 * 24 * 60 * 60
            };
            TransactionInput transactionInput = new TransactionInput
            {
                Target = TimelockContractAddress,
                Method = "SetDelay",
                Data = setDelayInput.ToByteString(),
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));
            var txRes = await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            
            UInt64Value delayAfter = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            delayAfter.Value.ShouldBe((uint)(4 * 24 * 60 * 60));
            
            var newDelayEvent = NewDelay.Parser
                .ParseFrom(txRes.TransactionResult.Logs.First(l => l.Name.Contains(nameof(NewDelay)))
                    .NonIndexed);
            newDelayEvent.Delay.ShouldBe(setDelayInput.Delay);
        }
        
        [Fact]
        public async Task SetDelayTests_NoPermission()
        {
            await InitializeTests();
            UInt64Value delay = await TimelockContractStub.GetDelay.CallAsync(new Empty());
            delay.Value.ShouldBe((uint)(2 * 24 * 60 * 60));
            SetDelayInput setDelayInput = new SetDelayInput
            {
                Delay = 4 * 24 * 60 * 60
            };
            try
            {
                await TimelockContractStub.SetDelay.SendAsync(setDelayInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("No Permission");
            }
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            var txRes = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
            
            var queueTransactionEvent = QueueTransaction.Parser
                .ParseFrom(txRes.TransactionResult.Logs.First(l => l.Name.Contains(nameof(QueueTransaction)))
                    .NonIndexed);
            queueTransactionEvent.Target.ShouldBe(transactionInput.Target);
            queueTransactionEvent.Method.ShouldBe(transactionInput.Method);
            queueTransactionEvent.Data.ShouldBe(transactionInput.Data);
            queueTransactionEvent.ExecuteTime.ShouldBe(transactionInput.ExecuteTime);
        }

        [Fact]
        public async Task QueueTransactionTests_NoPermission()
        {
            await ChangeAdminTests();
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            try
            {
                await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("No Permission");
            }
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = false
            };
            result.ShouldBe(expectResult);
        }
        
        [Fact]
        public async Task QueueTransactionTests_TimeLimit()
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
                ExecuteTime = _currentTime.AddSeconds(-(1 * 24 * 60 * 60))
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            try
            {
                await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("Estimated execution block must satisfy delay");
            }
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = false
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            var txRes = await TimelockContractStub.CancelTransaction.SendAsync(transactionInput);
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = false
            };
            result.ShouldBe(expectResult);
            
            var cancelTransactionEvent = CancelTransaction.Parser
                .ParseFrom(txRes.TransactionResult.Logs.First(l => l.Name.Contains(nameof(CancelTransaction)))
                    .NonIndexed);
            cancelTransactionEvent.Target.ShouldBe(transactionInput.Target);
            cancelTransactionEvent.Method.ShouldBe(transactionInput.Method);
            cancelTransactionEvent.Data.ShouldBe(transactionInput.Data);
            cancelTransactionEvent.ExecuteTime.ShouldBe(transactionInput.ExecuteTime);
        }
        
        [Fact]
        public async Task CancelTransactionTests_NoPermission()
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            Hash txnHash = HashHelper.ComputeFrom(transactionInput);
            await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);

            await ChangeAdminForTestPermission();
            
            try
            {
                await TimelockContractStub.CancelTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("No Permission");
            }
            
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));
            
            var txRes = await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            
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
            
            var executeTransactionEvent = ExecuteTransaction.Parser
                .ParseFrom(txRes.TransactionResult.Logs.First(l => l.Name.Contains(nameof(ExecuteTransaction)))
                    .NonIndexed);
            executeTransactionEvent.Target.ShouldBe(transactionInput.Target);
            executeTransactionEvent.Method.ShouldBe(transactionInput.Method);
            executeTransactionEvent.Data.ShouldBe(transactionInput.Data);
            executeTransactionEvent.ExecuteTime.ShouldBe(transactionInput.ExecuteTime);
        }
        
        [Fact]
        public async Task ExecuteTransactionTests_NoPermission()
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);

            await ChangeAdminForTestPermission();
            
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));

            try
            {
                await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("No Permission");
            }
            
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash.Output);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
        }
        
        [Fact]
        public async Task ExecuteTransactionTests_NotInQueue()
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));

            try
            {
                await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("executeTransaction: Transaction hasn't been queued");
            }
        }
        
        [Fact]
        public async Task ExecuteTransactionTests_NotReachExecuteTime()
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(2 * 24 * 60 * 60));

            try
            {
                await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("executeTransaction: Transaction hasn't surpassed time lock");
            }
            
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash.Output);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
        }
        
        [Fact]
        public async Task ExecuteTransactionTests_ExceedGracePeriod()
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
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            var txnHash = await TimelockContractStub.QueueTransaction.SendAsync(transactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(8 * 24 * 60 * 60));

            try
            {
                await TimelockContractStub.ExecuteTransaction.SendAsync(transactionInput);
            }
            catch (Exception e)
            {
                e.Message.ShouldContain("executeTransaction: Transaction is stale");
            }
            
            var result = await TimelockContractStub.GetTransaction.CallAsync(txnHash.Output);
            var expectResult = new BoolValue
            {
                Value = true
            };
            result.ShouldBe(expectResult);
        }
        
        private async Task InitializeAsync()
        {
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = TimelockContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = TimelockContractAddress,
                Symbol = "ELF",
                Amount = 1000_00000000
            });
        }
        
        private async Task ChangeAdminForTestPermission()
        {
            TransactionInput changeAdminTransactionInput = new TransactionInput
            {
                Target = TimelockContractAddress,
                Method = "ChangeAdmin",
                Data = new ChangeAdminInput() {Admin = UserAddress}.ToByteString(),
                ExecuteTime = _currentTime.AddSeconds(3 * 24 * 60 * 60)
            };
            await TimelockContractStub.QueueTransaction.SendAsync(changeAdminTransactionInput);
            BlockTimeProvider.SetBlockTime(_currentTime.AddSeconds(3 * 24 * 60 * 60));
            await TimelockContractStub.ExecuteTransaction.SendAsync(changeAdminTransactionInput);
        }
        
    }
    
}