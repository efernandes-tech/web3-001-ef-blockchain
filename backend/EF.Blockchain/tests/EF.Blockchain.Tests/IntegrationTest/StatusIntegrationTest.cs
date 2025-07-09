using EF.Blockchain.Server.Dtos;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class StatusIntegrationTest : BaseIntegrationTest
{
    [Fact]
    public async Task ServerTests_GetStatus_ShouldReturnStatus()
    {
        // Arrange

        // Act
        var result = await _flurl.Request("/status").GetJsonAsync<StatusDto>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Blocks);
        Assert.True(result.IsValid);
    }
}
