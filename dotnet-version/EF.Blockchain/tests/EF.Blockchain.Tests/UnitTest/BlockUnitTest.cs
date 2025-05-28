using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private readonly int ExampleDifficulty = 0;
    private readonly string ExampleMiner = "ef";
    private Block Genesis;

    public BlockUnitTest()
    {
        var genesisTx = TransactionMockFactory.Create(
            type: TransactionType.FEE,
            data: "Genesis Block"
        );

        Genesis = new Block(
            transactions: new List<Transaction> { genesisTx });
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: 1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });
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
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var blockInfo = new BlockInfo
        {
            Transactions = new List<Transaction> { transaction },
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
    public void BlockTests_IsValid_ShouldNotBeValid2Fee()
    {
        // Arrange
        var tx1 = TransactionMockFactory.Create(
            type: TransactionType.FEE,
            data: "fee1"
        );
        var tx2 = TransactionMockFactory.Create(
            type: TransactionType.FEE,
            data: "fee2"
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { tx1, tx2 }
        );
        block.Mine(ExampleDifficulty, ExampleMiner);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Equal("Too many fees.", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTx()
    {
        // Arrange
        var invalidTx = TransactionMockFactory.CreateInvalidData();
        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { invalidTx }
        );
        block.Mine(ExampleDifficulty, ExampleMiner);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid block due to invalid tx", valid.Message);
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
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: 1, previousHash: "abc",
            transactions: new List<Transaction> { transaction });

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidTimestamp()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: 1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });
        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Timestamp))!
            .SetValue(block, -1);
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidEmptyHash()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: 1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });
        block.Mine(ExampleDifficulty, ExampleMiner);

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoMined()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: 1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidData()
    {
        // Arrange
        var transaction = TransactionMockFactory.CreateInvalidData();
        var block = new Block(index: 1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidIndex()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           data: "Block 2"
        );
        var block = new Block(index: -1, previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
    }
}
