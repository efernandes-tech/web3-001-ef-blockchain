using EF.Blockchain.Domain;

namespace EF.Blockchain.UnitTest;

public class BlockUnitTest
{
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

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidFallbacks()
    {
        // Arrange
        var block = new Block();

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidPreviousHash()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: "abc", data: "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

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
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidHash()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "block 2");
        block.SetHash("");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidData()
    {
        // Arrange
        var block = new Block(index: 1, previousHash: Genesis.Hash, data: "");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidIndex()
    {
        // Arrange
        var block = new Block(index: -1, previousHash: Genesis.Hash, data: "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }
}
