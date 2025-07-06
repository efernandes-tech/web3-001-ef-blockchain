using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private const int _difficulty = 1;
    private const int _feePerTx = 1;
    private const string _exampleTx = "8eba6c75bbd12d9e21f657b76726312aad08f2d3a10aee52d2b1017e6248c186";

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

    private Block GetFullBlock()
    {
        var txInput = new TransactionInput(
            amount: 10,
            fromAddress: _loki.PublicKey,
            previousTx: _exampleTx
        );
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(
            toAddress: _thor.PublicKey,
            amount: 10
        );

        var regularTx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        var feeTx = new Transaction(
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
            transactions: new List<Transaction> { regularTx, feeTx }
        );

        block.Mine(_difficulty, _loki.PublicKey);

        return block;
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var block = GetFullBlock();

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidDifferentHash()
    {
        // Arrange
        var block = GetFullBlock();

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "abc");

        // Act
        var result = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

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
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

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
        var validation = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.True(validation.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValid2Fee()
    {
        // Arrange
        var block = GetFullBlock();

        var fee2 = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> { new TransactionOutput() }
        );

        block.Transactions.Add(fee2);

        block.Mine(_difficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Too many fees", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTx()
    {
        // Arrange
        var block = GetFullBlock();

        typeof(Transaction)
            .GetProperty(nameof(Transaction.Timestamp))!
            .SetValue(block.Transactions[0], -1);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

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
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidPreviousHash()
    {
        // Arrange
        var block = GetFullBlock();

        typeof(Block)
            .GetProperty(nameof(Block.PreviousHash))!
            .SetValue(block, "xyz");
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid previous hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidTimestamp()
    {
        // Arrange
        var block = GetFullBlock();

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Timestamp))!
            .SetValue(block, -1);
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid timestamp", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidEmptyHash()
    {
        // Arrange
        var block = GetFullBlock();

        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, "");

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid hash", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidNoMined()
    {
        // Arrange
        var block = GetFullBlock();

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Nonce))!
            .SetValue(block, 0);
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("No mined", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidTxInput()
    {
        // Arrange
        var block = GetFullBlock();

        typeof(TransactionInput)
            .GetProperty(nameof(TransactionInput.Amount))!
            .SetValue(block.Transactions[0].TxInputs![0], -1);

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid block due to invalid tx", valid.Message);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidInvalidIndex()
    {
        // Arrange
        var block = GetFullBlock();

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Index))!
            .SetValue(block, -1);
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(block, block.GetHash());

        // Act
        var valid = block.IsValid(_genesis.Hash, _genesis.Index, _difficulty, _feePerTx);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid index", valid.Message);
    }
}
