using EF.Blockchain.Domain;
using EF.Blockchain.Tests.IntegrationTest.Commons;
using EF.Blockchain.Tests.Mocks;
using Flurl.Http;

namespace EF.Blockchain.Tests.IntegrationTest;

public abstract class BaseIntegrationTest
{
    protected readonly IFlurlClient _flurl;
    protected readonly Wallet _loki;
    protected Domain.Blockchain _blockchain;
    protected readonly CustomWebApplicationFactory _factory;

    protected BaseIntegrationTest()
    {
        _factory = new CustomWebApplicationFactory();
        var client = _factory.CreateClient();
        _flurl = new FlurlClient(client);
        _loki = new Wallet();
        _blockchain = _factory._mockBlockchain;
    }

    protected IFlurlClient CreateFlurlClientWithBlockHash(string hash, Transaction? transaction = null)
    {
        _blockchain = BlockchainMockFactory.CreateWithBlocks(5);

        if (transaction != null)
        {
            var utxo = _blockchain.Blocks[0].Transactions[0];

            // Force invalid hash with reflection
            typeof(TransactionInput)
                .GetProperty(nameof(TransactionInput.PreviousTx))!
                .SetValue(transaction.TxInputs![0], utxo.Hash);

            transaction.TxInputs![0].Sign(BlockchainMockFactory.MockedPrivateKey);

            _blockchain.Mempool.Add(transaction);
        }

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(_blockchain.Blocks.Last(), hash);

        var factory = new CustomWebApplicationFactory(_blockchain);

        return new FlurlClient(factory.CreateClient());
    }

    public virtual void Dispose()
    {
        _flurl?.Dispose();
        _factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
