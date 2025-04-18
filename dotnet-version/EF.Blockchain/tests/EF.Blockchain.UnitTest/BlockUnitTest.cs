using EF.Blockchain.Domain;

namespace EF.Blockchain.UnitTest;

public class BlockUnitTest
{
    private Block Genesis;

    public BlockUnitTest()
    {
        Genesis = new Block(0, "", "Genesis Block");
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var block = new Block(1, Genesis.Hash, "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidPreviousHash()
    {
        // Arrange
        var block = new Block(1, "abc", "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidTimestamp()
    {
        // Arrange
        var block = new Block(1, Genesis.Hash, "block 2");
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
        var block = new Block(1, Genesis.Hash, "block 2");
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
        var block = new Block(1, Genesis.Hash, "");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidIndex()
    {
        // Arrange
        var block = new Block(-1, Genesis.Hash, "block 2");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index);

        // Assert
        Assert.False(valid.Success);
    }
}
