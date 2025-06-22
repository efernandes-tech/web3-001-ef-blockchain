using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;
using FluentAssertions;

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
        var transaction = TransactionMockFactory.Create();
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
        var transaction = TransactionMockFactory.Create();

        blockchain.Mempool.Add(transaction);

        var block = BlockMockFactory.Create(index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction });

        block.Mine(difficulty: 1, miner: "ef");

        blockchain.AddBlock(block);

        var transaction2 = TransactionMockFactory.Create();

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
    public void BlockchainTests_AddTransaction_ShouldAddTransaction()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var tx = TransactionMockFactory.Create();

        // Act
        var validation = blockchain.AddTransaction(tx);

        // Assert
        validation.Success.Should().BeTrue();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionInvalidTx()
    {
        var blockchain = new Domain.Blockchain();

        var tx = TransactionMockFactory.CreateInvalidTxInput();

        var validation = blockchain.AddTransaction(tx);
        validation.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionDuplicatedInBlockchain()
    {
        var blockchain = new Domain.Blockchain();

        var tx = TransactionMockFactory.Create();

        var block = BlockMockFactory.Create(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx });

        blockchain.Blocks.Add(block);

        var validation = blockchain.AddTransaction(tx);
        validation.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionDuplicatedInMempool()
    {
        var blockchain = new Domain.Blockchain();

        var tx = TransactionMockFactory.Create();

        blockchain.Mempool.Add(tx);

        var validation = blockchain.AddTransaction(tx);
        validation.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldGetTransactionMempool()
    {
        var blockchain = new Domain.Blockchain();

        var tx = TransactionMockFactory.Create();

        blockchain.Mempool.Add(tx);

        var result = blockchain.GetTransaction(tx.Hash);
        result.MempoolIndex.Should().Be(0);
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldGetTransactionBlockchain()
    {
        var blockchain = new Domain.Blockchain();

        var tx = TransactionMockFactory.Create();

        var block = BlockMockFactory.Create(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx });

        blockchain.Blocks.Add(block);

        var result = blockchain.GetTransaction(tx.Hash);

        result.BlockIndex.Should().Be(1);
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldNotGetTransaction()
    {
        var blockchain = new Domain.Blockchain();

        var result = blockchain.GetTransaction("xyz");

        result.BlockIndex.Should().Be(-1);
        result.MempoolIndex.Should().Be(-1);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();
        var transaction = TransactionMockFactory.Create();

        blockchain.Mempool.Add(transaction);

        var block = BlockMockFactory.Create(
            index: 1,
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
        var transaction = TransactionMockFactory.Create();
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
        blockchain.Mempool.Add(new Transaction());

        // Act
        var info = blockchain.GetNextBlock();

        // Assert
        Assert.Equal(1, info is not null ? info.Index : 0);
    }

    [Fact]
    public void BlockchainTests_GetNextBlock_ShouldNotGetNextBlockInfo()
    {
        // Arrange
        var blockchain = new Domain.Blockchain();

        // Act
        var info = blockchain.GetNextBlock();

        // Assert
        Assert.Null(info);
    }
}
