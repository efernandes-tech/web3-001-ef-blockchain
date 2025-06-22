using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockchainMockFactory
{
    public static Domain.Blockchain CreateWithGenesis()
    {
        return new Domain.Blockchain();
    }

    public static Domain.Blockchain CreateWithBlocks(int count, bool addExtraTx = true)
    {
        if (count < 1)
            throw new ArgumentException("Must create at least 1 block");

        var chain = new Domain.Blockchain();

        for (int i = 1; i < count; i++)
        {
            var last = chain.GetLastBlock();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var index = chain.NextIndex;
            var transaction = new Transaction(timestamp: timestamp, txInput: new TransactionInput());

            chain.Mempool.Add(transaction);

            var block = new Block(
                index,
                last.Hash,
                new List<Transaction> { transaction },
                timestamp,
                hash: null
            );
            block.Mine(
                chain.GetDifficulty(),
                miner: "ef"
            );

            chain.AddBlock(block);
        }

        if (addExtraTx)
        {
            var extraTransaction = new Transaction(txInput: new TransactionInput());
            chain.Mempool.Add(extraTransaction);
        }

        return chain;
    }
}
