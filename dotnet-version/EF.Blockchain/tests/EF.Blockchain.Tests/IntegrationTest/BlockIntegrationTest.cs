using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class BlockIntegrationTest : BaseIntegrationTest
{
    [Fact]
    public async Task ServerTests_GetBlocksByIndex_ShouldGetGenesis()
    {
        // Arrange

        // Act
        var response = await _flurl.Request("/blocks/0").GetJsonAsync<BlockResponse>();

        // Assert
        Assert.Equal(0, response.Index);
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
        Assert.Equal("abc", response.Hash);
    }

    [Fact]
    public async Task ServerTests_GetBlocksInvalidIndex_ShouldNotGetBlock()
    {
        // Arrange

        // Act
        var response = await _flurl.Request("/blocks/-1").AllowHttpStatus("404").GetAsync();

        // Assert
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task ServerTests_PostValidBlock_ShouldAddBlock()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var txInput = new TransactionInput(
            fromAddress: BlockchainMockFactory.MockedPublicKey,
            amount: 1, previousTx: "previousTxMock");

        txInput.Sign(BlockchainMockFactory.MockedPrivateKey);

        var transaction = new Transaction(
            timestamp: timestamp,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: BlockchainMockFactory.MockedPublicKey, amount: 1) });

        var transactionFee = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: BlockchainMockFactory.MockedPublicKey, amount: 1) },
            timestamp: timestamp);

        var flurl = CreateFlurlClientWithBlockHash("abc", transaction);

        var index = 1;

        var utxo = _blockchain.Blocks[0].Transactions[0];

        // Force invalid hash with reflection
        typeof(TransactionInput)
            .GetProperty(nameof(TransactionInput.PreviousTx))!
            .SetValue(transaction.TxInputs![0], utxo.Hash);
        typeof(Transaction)
            .GetProperty(nameof(Transaction.Hash))!
            .SetValue(transaction, transaction.GetHash());

        transaction.TxInputs![0].Sign(BlockchainMockFactory.MockedPrivateKey);

        var block = new Block(
            index,
            previousHash: "abc",
            new List<Transaction> { transaction, transactionFee },
            timestamp);

        block.Mine(difficulty: 2, miner: BlockchainMockFactory.MockedPublicKey);

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
        Assert.Equal(1, response.Index);
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
        Assert.Equal(422, response.StatusCode);
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
        Assert.Equal(400, response.StatusCode);
    }

    private record BlockResponse(
        int Index,
        string Hash,
        string PreviousHash,
        string Data,
        long Timestamp
    );
}
