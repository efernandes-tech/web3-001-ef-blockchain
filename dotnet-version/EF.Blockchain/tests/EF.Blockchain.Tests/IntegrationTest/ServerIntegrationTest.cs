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
        result.blocks.Should().Be(1);
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
        Assert.Equal(1, response.Index);
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
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var txInput = new TransactionInput(
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000);
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);
        var transaction = new Transaction(timestamp: timestamp, txInput: txInput, to: TransactionMockFactory.MockedPublicKeyTo);

        var flurl = CreateFlurlClientWithBlockHash("abc", transaction);

        var index = 1;
        var previousHash = "abc";

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
        response.Index.Should().Be(1);
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

    [Fact]
    public async Task ServerTests_GetTransaction_ShouldGetTransaction()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var txInput = new TransactionInput(
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000);
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);
        var tx = new Transaction(timestamp: timestamp, txInput: txInput);

        var blockchain = BlockchainMockFactory.CreateWithBlocks(3, false);
        blockchain.Mempool.Add(tx);

        var factory = new CustomWebApplicationFactory(blockchain);
        var client = factory.CreateClient();
        var flurl = new FlurlClient(client);

        // Act
        var response = await flurl.Request("/transactions/" + tx.Hash).GetAsync();
        var body = await response.GetJsonAsync<TransactionSearchResponse>();

        // Assert
        response.StatusCode.Should().Be(200);
        body.MempoolIndex.Should().Be(2);
    }

    [Fact]
    public async Task ServerTests_PostTransaction_ShouldAddTx()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000);
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);
        var tx = new Transaction(timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds(), txInput: txInput, to: TransactionMockFactory.MockedPublicKeyTo);

        // Act
        var response = await _flurl.Request("/transactions").PostJsonAsync(tx);

        // Assert
        response.StatusCode.Should().Be(201);
    }

    private record TransactionSearchResponse(
        Transaction? Transaction,
        int MempoolIndex,
        int BlockIndex
    );

    private record StatusResponse(
        int blocks,
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

    private record TransactionResponse(
        Transaction Transaction
    );

    private IFlurlClient CreateFlurlClientWithBlockHash(string hash, Transaction? transaction = null)
    {
        var blockchain = BlockchainMockFactory.CreateWithBlocks(5);

        if (transaction is not null)
        {
            blockchain.Mempool.Add(transaction);
        }

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(blockchain.Blocks[blockchain.Blocks.Count - 1], hash);

        var factory = new CustomWebApplicationFactory(blockchain);
        var client = factory.CreateClient();

        return new FlurlClient(client);
    }
}
