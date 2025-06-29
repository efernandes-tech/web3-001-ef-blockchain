using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private readonly int _difficulty = 1;
    private readonly Wallet _loki;
    private readonly Wallet _thor;
    private Block _genesis;

    public BlockUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();

        var genesisTx = new Transaction(
            type: TransactionType.FEE
        );

        _genesis = new Block(
            transactions: new List<Transaction> { genesisTx });
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
               new TransactionOutput(
                   toAddress: _loki.PublicKey,
                   amount: 1
               )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidDifferentHash()
    {
        // Arrange
        var fee = new Transaction(TransactionType.FEE, txOutputs: new List<TransactionOutput>
        {
            new TransactionOutput(_loki.PublicKey, 1)
        });

        var block = new Block(1, _genesis.Hash, new List<Transaction> { fee });
        block.Mine(_difficulty, _loki.PublicKey);

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "abc");

        // Act
        var result = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoFee()
    {
        // Arrange
        var transaction = new Transaction(
           type: TransactionType.REGULAR
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transaction });

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("No fee tx", valid.Message);
    }

    [Fact]
    public void BlockTests_FromBlockInfo_ShouldCreateFromBlockInfo()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput> {
               new TransactionOutput(
                   toAddress: _loki.PublicKey,
                   amount: 1
               )
           }
        );

        var blockInfo = new BlockInfo
        {
            Index = 1,
            PreviousHash = _genesis.GetHash(),
            Transactions = new List<Transaction> { transactionFee },
            Difficulty = _difficulty,
            FeePerTx = 1,
            MaxDifficulty = 62,
        };

        // Act
        var block = Block.FromBlockInfo(blockInfo);
        block.Mine(_difficulty, _loki.PublicKey);
        var validation = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.True(validation.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValid2Fee()
    {
        // Arrange
        var fee1 = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput()
            }
        );
        var fee2 = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> { new TransactionOutput() }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { fee1, fee2 }
        );
        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Too many fees", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTx()
    {
        // Arrange
        var invalidTx = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput(
                    amount: -1)
            }
        );

        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
               new TransactionOutput(
                   toAddress: _loki.PublicKey,
                   amount: 1
               )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { invalidTx, transactionFee }
        );

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

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
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidPreviousHash()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _loki.PublicKey,
                     amount: 1
                )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: "abc",
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid previous hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTimestamp()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _loki.PublicKey,
                     amount: 1
                )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Timestamp))!
            .SetValue(block, -1);
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid timestamp", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidEmptyHash()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _loki.PublicKey,
                     amount: 1
                )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "");

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoMined()
    {
        // Arrange
        var transactionFee = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 1
                )
            }
        );

        var block = new Block(
            index: 1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee },
            nonce: 0,
            miner: _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("No mined", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeTxInput()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _loki.PublicKey,
                     amount: -1
                )
           }
        );

        var block = new Block(
            index: -1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid block due to invalid tx", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidIndex()
    {
        // Arrange
        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _loki.PublicKey,
                     amount: 1
                )
           }
        );

        var block = new Block(
            index: -1,
            previousHash: _genesis.Hash,
            transactions: new List<Transaction> { transactionFee });

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid index", valid.Message);
    }
}
