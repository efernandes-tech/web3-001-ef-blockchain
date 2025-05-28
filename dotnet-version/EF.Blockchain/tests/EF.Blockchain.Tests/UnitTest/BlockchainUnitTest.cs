using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockchainUnitTest
{
    [Fact]
    public void BlockchainTests_Constructor_ShouldHasGenesisBlocks()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var blockCount = blockchain.Blocks.Count;

        // Assert
        Assert.Equal(1, blockCount);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldBeValidGenesis()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var valid = blockchain.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldBeValidTwoBlocks()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var transaction = TransactionMockFactory.Create(data: "Block 2");
        blockchain.AddBlock(
            BlockMockFactory.Create(index: 1,
                previousHash: blockchain.GetLastBlock().Hash,
                transactions: new List<Transaction> { transaction })
        );

        // Act
        var valid = blockchain.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldNotBeValid()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var transaction = TransactionMockFactory.Create(data: "Block 2");
        var block = BlockMockFactory.Create(index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction });
        block.Mine(difficulty: 1, miner: "ef");

        blockchain.AddBlock(block);

        var transaction2 = TransactionMockFactory.Create(data: "A transfer X to B");

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Transactions))!
            .SetValue(blockchain.Blocks[1], new List<Transaction> { transaction2 });

        // Act
        var valid = blockchain.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var transaction = TransactionMockFactory.Create(data: "Block 2");
        var block = BlockMockFactory.Create(index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction });
        block.Mine(difficulty: 1, miner: "ef");

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void BlockchainTests_GetBlock_ShouldGetBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var block = blockchain.GetBlock(blockchain.GetLastBlock().Hash);

        // Assert
        Assert.NotNull(block);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldNotAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var transaction = TransactionMockFactory.Create(data: "Block 2");
        var block = BlockMockFactory.Create(index: -1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction });

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void BlockchainTests_GetNextBlock_ShouldGetNextBlockInfo()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var info = blockchain.GetNextBlock();

        // Assert
        Assert.Equal(1, info.Index);
    }
}
