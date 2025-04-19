using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockchainMockFactory
{
    public static Domain.Blockchain CreateWithGenesis()
    {
        return new Domain.Blockchain();
    }

    public static Domain.Blockchain CreateWithBlocks(int count)
    {
        if (count < 1)
            throw new ArgumentException("Must create at least 1 block");

        var chain = new Domain.Blockchain();

        for (int i = 1; i < count; i++)
        {
            var last = chain.GetLastBlock();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var index = chain.NextIndex;

            var block = new Block(
                index,
                last.Hash,
                $"Block {i}",
                timestamp,
                hash: null
            );

            chain.AddBlock(block);
        }

        return chain;
    }
}
