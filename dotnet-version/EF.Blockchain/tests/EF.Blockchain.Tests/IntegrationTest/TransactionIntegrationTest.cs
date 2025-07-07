using EF.Blockchain.Domain;
using EF.Blockchain.Tests.IntegrationTest.Commons;
using EF.Blockchain.Tests.Mocks;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public class TransactionIntegrationTest : BaseIntegrationTest
{
    [Fact]
    public async Task ServerTests_GetTransaction_ShouldGetTransaction()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var txInput = new TransactionInput(
            fromAddress: BlockchainMockFactory.MockedPublicKey,
            amount: 1000,
            previousTx: "previousTxMock");

        txInput.Sign(BlockchainMockFactory.MockedPrivateKey);

        var tx = new Transaction(
            timestamp: timestamp,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: BlockchainMockFactory.MockedPublicKey, amount: 1000) });

        var blockchain = BlockchainMockFactory.CreateWithBlocks(3, false);

        blockchain.Mempool.Add(tx);

        var factory = new CustomWebApplicationFactory(blockchain);
        var client = factory.CreateClient();
        var flurl = new FlurlClient(client);

        // Act
        var response = await flurl.Request("/transactions/" + tx.Hash).GetAsync();
        var body = await response.GetJsonAsync<TransactionSearchResponse>();

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, body.MempoolIndex);
    }

    [Fact]
    public async Task ServerTests_PostTransaction_ShouldAddTx()
    {
        // Arrange
        var utxo = _blockchain.Blocks[0].Transactions[0];
        _blockchain.Mempool.Clear();

        var txInput = new TransactionInput(
            fromAddress: BlockchainMockFactory.MockedPublicKey,
            amount: 1,
            previousTx: utxo.Hash);

        txInput.Sign(BlockchainMockFactory.MockedPrivateKey);

        var tx = new Transaction(
            timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: _loki.PublicKey, amount: 1) });

        // Act
        var response = await _flurl.Request("/transactions").PostJsonAsync(tx);

        // Assert
        Assert.Equal(201, response.StatusCode);
    }

    private record TransactionSearchResponse(
        Transaction? Transaction,
        int MempoolIndex,
        int BlockIndex
    );
}
