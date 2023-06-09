using System.Linq;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.Timelock
{
    public partial class TimelockContractTests : TestBase
    {
        [Fact]
        public async Task PlayTests()
        {
            // Prepare awards.
            await TokenContractStub.Transfer.SendAsync(new TransferInput
            {
                To = DAppContractAddress,
                Symbol = "ELF",
                Amount = 100_00000000
            });

            await TokenContractStub.Create.SendAsync(new CreateInput
            {
                Symbol = "CARD",
                TokenName = "Bingo Card",
                Decimals = 0,
                Issuer = DAppContractAddress,
                IsBurnable = true,
                TotalSupply = long.MaxValue
            });

            await TimelockContractStub.Register.SendAsync(new Empty());

            await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Symbol = "CARD",
                Amount = long.MaxValue
            });

            // Now I have player information.
            var address = Address.FromPublicKey(DefaultKeyPair.PublicKey);
            {
                var playerInformation = await TimelockContractStub.GetPlayerInformation.CallAsync(address);
                playerInformation.Seed.Value.ShouldNotBeEmpty();
                playerInformation.RegisterTime.ShouldNotBeNull();
            }

            // Play.
            var txResult = (await TokenContractStub.Approve.SendAsync(new ApproveInput
            {
                Spender = DAppContractAddress,
                Symbol = "ELF",
                Amount = 10000
            })).TransactionResult;
            txResult.Status.ShouldBe(TransactionResultStatus.Mined);

            await TimelockContractStub.Play.SendAsync(new Int64Value {Value = 10000});

            Hash playId;
            {
                var playerInformation = await TimelockContractStub.GetPlayerInformation.CallAsync(address);
                playerInformation.Bouts.ShouldNotBeEmpty();
                playId = playerInformation.Bouts.First().PlayId;
            }

            // Mine 7 more blocks.
            for (var i = 0; i < 7; i++)
            {
                await TimelockContractStub.Bingo.SendWithExceptionAsync(playId);
            }

            await TimelockContractStub.Bingo.SendAsync(playId);

            var award = await TimelockContractStub.GetAward.CallAsync(playId);
            award.Value.ShouldNotBe(0);
        }
    }
    
}