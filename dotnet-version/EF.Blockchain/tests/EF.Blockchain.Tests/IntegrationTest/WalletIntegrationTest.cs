using EF.Blockchain.Tests.Mocks;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class WalletIntegrationTest : BaseIntegrationTest
{
    [Fact]
    public async Task ServerTests_GetWallet_ShouldGetBalance()
    {
        // Arrange
        var walletAddress = BlockchainMockFactory.MockedPublicKey;

        // Act
        var response = await _flurl.Request($"/wallets/{walletAddress}").GetAsync();
        var json = await response.GetJsonAsync<WalletResponse>();

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(630, json.Balance);
    }

    private record WalletResponse(int Balance);
}
