using EF.Blockchain.Tests.IntegrationTest.Commons;
using FluentAssertions;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class ServerIntegrationTest
{
    private readonly IFlurlClient _flurl;

    public ServerIntegrationTest()
    {
        var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        _flurl = new FlurlClient(client);
    }

    [Fact]
    public async Task ServerTests_Status_GetStatus()
    {
        // Arrange

        // Act
        var result = await _flurl.Request("/status").GetJsonAsync<StatusResponse>();

        // Assert
        result.Should().NotBeNull();
        result.NumberOfBlocks.Should().Be(3);
        result.IsValid.Success.Should().BeTrue();
    }

    private class StatusResponse
    {
        public int NumberOfBlocks { get; set; }
        public ValidationResponse IsValid { get; set; } = new();
        public object? LastBlock { get; set; }
    }

    private class ValidationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
