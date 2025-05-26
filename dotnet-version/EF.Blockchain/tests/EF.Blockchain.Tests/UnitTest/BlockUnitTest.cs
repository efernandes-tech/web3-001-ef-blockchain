using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private readonly int ExampleDifficulty = 0;
    private readonly string ExampleMiner = "ef";
    private Block Genesis;

    public BlockUnitTest()
    {
        Genesis = new Block(data: "Genesis Block");
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "block 2");
        block.Mine(ExampleDifficulty, ExampleMiner);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_FromBlockInfo_ShouldCreateFromBlockInfo()
    {
        // Arrange
        var exampleDifficulty = 0;
        var exampleMiner = "efernandes";
        var blockInfo = new BlockInfo
        {
            Data = "Block 2",
            Difficulty = exampleDifficulty,
            FeePerTx = 1,
            Index = 1,
            MaxDifficulty = 62,
            PreviousHash = Genesis.GetHash()
        };

        // Act
        var block = Block.FromBlockInfo(blockInfo);
        block.Mine(exampleDifficulty, exampleMiner);
        var validation = block.IsValid(Genesis.Hash, Genesis.Index, exampleDifficulty);

        // Assert
        Assert.True(validation.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidFallbacks()
    {
        // Arrange
        var block = new Block();

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidPreviousHash()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: "abc", data: "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidTimestamp()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "block 2");
        block.SetTimestamp(-1);
        block.SetHash(block.GetHash());

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidEmptyHash()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "block 2");
        block.Mine(ExampleDifficulty, ExampleMiner);
        block.SetHash("");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoMined()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidData()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidIndex()
    {
        // Arrange
        var block = new Block(index: -1, previousHash: Genesis.Hash, data: "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }
}
