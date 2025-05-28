using EF.Blockchain.Domain;
using EF.Blockchain.Tests.IntegrationTest.Commons;
using EF.Blockchain.Tests.Mocks;
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
    public async Task ServerTests_GetStatus_ShouldReturnStatus()
    {
        // Arrange

        // Act
        var result = await _flurl.Request("/status").GetJsonAsync<StatusResponse>();

        // Assert
        result.Should().NotBeNull();
        result.NumberOfBlocks.Should().Be(3);
        result.IsValid.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ServerTests_GetBlocksByIndex_ShouldGetGenesis()
    {
        // Arrange

        // Act
        var response = await _flurl.Request("/blocks/0").GetJsonAsync<BlockResponse>();

        // Assert
        response.Index.Should().Be(0);
    }

    [Fact]
    public async Task ServerTests_GetNextBlock_ShouldReturnNextBlockInfo()
    {
        // Arrange

        // Act
        var response = await _flurl
            .Request("/blocks/next")
            .GetJsonAsync<BlockInfo>();

        // Assert
        Assert.Equal(3, response.Index);
    }

    [Fact]
    public async Task ServerTests_GetBlocksByHash_ShouldGetBlock()
    {
        // Arrange
        var flurl = CreateFlurlClientWithBlockHash("abc");

        // Act
        var response = await flurl.Request("/blocks/abc").GetJsonAsync<BlockResponse>();

        // Assert
        response.Hash.Should().Be("abc");
    }

    [Fact]
    public async Task ServerTests_GetBlocksInvalidIndex_ShouldNotGetBlock()
    {
        // Arrange

        // Act
        var response = await _flurl.Request("/blocks/-1").AllowHttpStatus("404").GetAsync();

        // Assert
        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ServerTests_PostValidBlock_ShouldAddBlock()
    {
        // Arrange
        var flurl = CreateFlurlClientWithBlockHash("abc");

        var index = 5;
        var previousHash = "abc";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var transaction = new Transaction(timestamp: timestamp, data: "test");
        var block = new Block(index, previousHash, new List<Transaction> { transaction }, timestamp);
        block.Mine(difficulty: 1, miner: "ef");

        var postBlock = new
        {
            block.Index,
            block.PreviousHash,
            block.Transactions,
            block.Timestamp,
            block.Nonce,
            block.Miner,
            block.Hash
        };

        // Act
        var response = await flurl.Request("/blocks")
            .PostJsonAsync(block)
            .ReceiveJson<BlockResponse>();

        // Assert
        response.Index.Should().Be(5);
    }

    [Fact]
    public async Task ServerTests_PostEmptyBlock_ShouldNotAddBlock()
    {
        // Arrange

        // Act
        var response = await _flurl.Request("/blocks")
            .AllowHttpStatus("422")
            .PostJsonAsync(new { });

        // Assert
        response.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task ServerTests_PostInvalidBlock_ShouldNotAddBlock()
    {
        // Arrange
        var block = new
        {
            index = -1,
            previousHash = "",
            data = "",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            hash = "wrong"
        };

        // Act
        var response = await _flurl.Request("/blocks")
            .AllowHttpStatus("400")
            .PostJsonAsync(block);

        // Assert
        response.StatusCode.Should().Be(400);
    }

    private record StatusResponse(
        int NumberOfBlocks,
        ValidationResponse IsValid,
        object? LastBlock
    );

    private record ValidationResponse(
        bool Success,
        string? Message
    );

    private record BlockResponse(
        int Index,
        string Hash,
        string PreviousHash,
        string Data,
        long Timestamp
    );

    private IFlurlClient CreateFlurlClientWithBlockHash(string hash)
    {
        var blockchain = BlockchainMockFactory.CreateWithBlocks(5);

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(blockchain.Blocks[blockchain.Blocks.Count - 1], hash);

        var factory = new CustomWebApplicationFactory(blockchain);
        var client = factory.CreateClient();

        return new FlurlClient(client);
    }
}
