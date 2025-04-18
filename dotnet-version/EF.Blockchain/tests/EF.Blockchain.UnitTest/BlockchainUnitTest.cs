namespace EF.Blockchain.UnitTest;

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
    public void BlockchainTests_Constructor_ShouldBeValidGenesis()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var isValid = blockchain.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void BlockchainTests_Constructor_ShouldBeValidTwoBlocks()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        blockchain.AddBlock(new Domain.Block(1, blockchain.GetLastBlock().Hash, "block 2"));

        // Act
        var isValid = blockchain.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void BlockchainTests_Constructor_ShouldNotBeValid()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        blockchain.AddBlock(new Domain.Block(1, blockchain.GetLastBlock().Hash, "block 2"));
        blockchain.Blocks[1].SetData("A transfer 2 for B");

        // Act
        var isValid = blockchain.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void BlockchainTests_Constructor_ShouldAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var result = blockchain.AddBlock(new Domain.Block(1, blockchain.GetLastBlock().Hash, "block 2"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BlockchainTests_Constructor_ShouldNotAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var block = new Domain.Block(-1, blockchain.GetLastBlock().Hash, "block 2");

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        Assert.False(result);
    }
}
