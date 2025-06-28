using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockchainMockFactory
{
    public static Domain.Blockchain CreateWithGenesis()
    {
        return new Domain.Blockchain(TransactionMockFactory.MockedPublicKey);
    }

    public static Domain.Blockchain CreateWithBlocks(int count, bool addExtraTx = true)
    {
        if (count < 1)
            throw new ArgumentException("Must create at least 1 block");

        var chain = new Domain.Blockchain(TransactionMockFactory.MockedPublicKey);

        for (int i = 1; i < count; i++)
        {
            var last = chain.GetLastBlock();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var index = chain.NextIndex;
            var transactionInput = new TransactionInput(
                fromAddress: TransactionMockFactory.MockedPublicKey,
                amount: 10
            );
            transactionInput.Sign(TransactionMockFactory.MockedPrivateKey);
            var transactionOutput = new TransactionOutput(
                toAddress: TransactionMockFactory.MockedPublicKeyTo,
                amount: 10
            );
            var transaction = new Transaction(
                timestamp: timestamp,
                txInputs: new List<TransactionInput> { transactionInput },
                txOutputs: new List<TransactionOutput> { transactionOutput }
            );
            var transactionFee = new Transaction(
                type: TransactionType.FEE,
                txOutputs: new List<TransactionOutput> { transactionOutput });

            chain.Mempool.Add(transaction);

            var block = new Block(
                index,
                last.Hash,
                new List<Transaction> { transaction, transactionFee },
                timestamp,
                hash: null
            );
            block.Mine(
                chain.GetDifficulty(),
                miner: TransactionMockFactory.MockedPublicKey
            );

            chain.AddBlock(block);
        }

        if (addExtraTx)
        {
            var extraTransaction = new Transaction(txInputs: new List<TransactionInput> { new TransactionInput() });
            chain.Mempool.Add(extraTransaction);
        }

        return chain;
    }
}
