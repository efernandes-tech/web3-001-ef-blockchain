using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private readonly int ExampleDifficulty = 1;
    private readonly string ExampleMiner = "ef";
    private Block Genesis;

    public BlockUnitTest()
    {
        var genesisTx = TransactionMockFactory.Create(
            type: TransactionType.FEE
        );

        Genesis = new Block(
            transactions: new List<Transaction> { genesisTx });
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(
               toAddress: TransactionMockFactory.MockedPublicKey,
               amount: 1
           )
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidFeeForOther()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(
               toAddress: ExampleMiner,
               amount: 1
           )
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid fee tx: different from miner", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoFee()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("No fee tx", valid.Message);
    }

    [Fact]
    public void BlockTests_FromBlockInfo_ShouldCreateFromBlockInfo()
    {
        // Arrange
        var exampleMiner = TransactionMockFactory.MockedPublicKey;

        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR
        );
        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: exampleMiner, amount: 1)
        );
        var blockInfo = new BlockInfo
        {
            Index = 1,
            PreviousHash = Genesis.GetHash(),
            Transactions = new List<Transaction> { transaction, transactionFee },
            Difficulty = ExampleDifficulty,
            FeePerTx = 1,
            MaxDifficulty = 62,
        };

        // Act
        var block = Block.FromBlockInfo(blockInfo);
        block.Mine(ExampleDifficulty, exampleMiner);
        var validation = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.True(validation.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValid2Fee()
    {
        // Arrange
        var tx1 = TransactionMockFactory.Create(
            type: TransactionType.FEE,
            transactionInput: new TransactionInput()
        );
        var tx2 = TransactionMockFactory.Create(
            type: TransactionType.FEE,
            transactionInput: new TransactionInput()
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
        Assert.Contains("Too many fees", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTx()
    {
        // Arrange
        var invalidTx = TransactionMockFactory.CreateInvalidTxInput();

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { invalidTx, transactionFee }
        );

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

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
    public void BlockTests_IsValid_ShouldNotBeValidInvalidPreviousHash()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR,
           transactionInput: new TransactionInput()
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: "abc",
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid previous hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTimestamp()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR,
           transactionInput: new TransactionInput()
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

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
        Assert.Contains("Invalid timestamp", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidEmptyHash()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR,
           transactionInput: new TransactionInput()
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "");

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoMined()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR,
           transactionInput: new TransactionInput()
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee },
            nonce: 0,
            miner: TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("No mined", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeTxInput()
    {
        // Arrange
        var transaction = TransactionMockFactory.CreateInvalidTxInput();

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid block due to invalid tx", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidIndex()
    {
        // Arrange
        var transaction = TransactionMockFactory.Create(
           type: TransactionType.REGULAR,
           transactionInput: new TransactionInput()
        );

        var transactionFee = TransactionMockFactory.Create(
           type: TransactionType.FEE,
           transactionOutput: new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKey, amount: 1)
        );

        var block = new Block(
            index: -1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, TransactionMockFactory.MockedPublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid index", valid.Message);
    }
}
