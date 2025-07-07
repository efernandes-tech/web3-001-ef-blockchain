using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class StatusIntegrationTest : BaseIntegrationTest
{
    [Fact]
    public async Task ServerTests_GetStatus_ShouldReturnStatus()
    {
        // Arrange

        // Act
        var result = await _flurl.Request("/status").GetJsonAsync<StatusResponse>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.blocks);
        Assert.True(result.IsValid.Success);
    }

    private record StatusResponse(
        int blocks,
        ValidationResponse IsValid,
        object? LastBlock
    );

    private record ValidationResponse(
        bool Success,
        string? Message
    );
}
