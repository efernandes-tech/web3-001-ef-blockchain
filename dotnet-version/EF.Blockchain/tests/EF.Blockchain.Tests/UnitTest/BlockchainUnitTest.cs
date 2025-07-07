using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockchainUnitTest
{
    private readonly Wallet _loki;
    private readonly Wallet _thor;

    public BlockchainUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();
    }

    [Fact]
    public void BlockchainTests_Constructor_ShouldHasGenesisBlocks()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var blockCount = blockchain.Blocks.Count;

        // Assert
        Assert.Equal(1, blockCount);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldBeValidGenesis()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var valid = blockchain.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldBeValidTwoBlocks()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput()
            }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction }
        );

        // Act
        blockchain.AddBlock(block);
        var valid = blockchain.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockchainTests_IsValid_ShouldNotBeValid()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTx");

        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: "toAddress", amount: 10)
            }
        );

        blockchain.Mempool.Add(transaction);

        var txFee = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: _loki.PublicKey, amount: 10)
            }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction, txFee }
        );

        block.Mine(difficulty: 2, miner: _loki.PublicKey);

        blockchain.AddBlock(block);

        // Act
        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Index))!
            .SetValue(
                blockchain.Blocks[1], -1);

        var valid = blockchain.IsValid();

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid block", valid.Message);
        Assert.Contains("Invalid index", valid.Message);
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldAddTransaction()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);
        var txo = blockchain.Blocks[0].Transactions[0];

        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: txo.Hash);

        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: "toAddress", amount: 10)
            }
        );

        // Act
        var validation = blockchain.AddTransaction(transaction);

        // Assert
        validation.Success.Should().BeTrue();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionInvalidUTXO()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var tx = new Transaction(
            txInputs: new List<TransactionInput>
            {
                new TransactionInput(
                    amount: 10,
                    previousTx: "wrong",
                    fromAddress: _loki.PublicKey,
                    signature: "abc"
                )
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    amount: 10,
                    toAddress : "abc"
                )
            }
        );

        // Act
        var result = blockchain.AddTransaction(tx);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionPendingTx()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);
        var txo = blockchain.Blocks[0].Transactions[0];

        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: txo.Hash);

        txInput.Sign(_loki.PrivateKey);

        var tx1 = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: "toAddress", amount: 10)
            }
        );

        blockchain.AddTransaction(tx1); // First tx added

        var tx2 = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: "toAddress", amount: 10)
            }
        );

        // Act
        var validation = blockchain.AddTransaction(tx2);

        // Assert
        validation.Success.Should().BeFalse();
        validation.Message.Should().Be("This wallet has a pending transaction");
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionInvalidTx()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var tx = new Transaction(
            type: TransactionType.REGULAR,
            timestamp: -1,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput()
            }
        );

        // Act
        var validation = blockchain.AddTransaction(tx);

        // Assert
        validation.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_AddTransaction_ShouldNotAddTransactionDuplicatedInBlockchain()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var tx = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput()
            }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx });

        blockchain.Blocks.Add(block);

        // Act
        var validation = blockchain.AddTransaction(tx);

        // Assert
        validation.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldGetTransactionMempool()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var tx = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput()
            }
        );

        blockchain.Mempool.Add(tx);

        // Act
        var result = blockchain.GetTransaction(tx.Hash);

        // Assert
        result.MempoolIndex.Should().Be(0);
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldGetTransactionBlockchain()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var tx = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput()
            }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx });

        blockchain.Blocks.Add(block);

        // Act
        var result = blockchain.GetTransaction(tx.Hash);

        // Assert
        result.BlockIndex.Should().Be(1);
    }

    [Fact]
    public void BlockchainTests_GetTransaction_ShouldNotGetTransaction()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var result = blockchain.GetTransaction("xyz");

        // Assert
        result.BlockIndex.Should().Be(-1);
        result.MempoolIndex.Should().Be(-1);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldAddBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTx");

        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(toAddress: "toAddress", amount: 10)
            }
        );

        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
               new TransactionOutput(toAddress: _loki.PublicKey, amount: 10)
           }
        );

        blockchain.Mempool.Add(transaction);

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(difficulty: 2, miner: _loki.PublicKey);

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldNotAddBlockInvalidMempool()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Add 2 transactions to mempool, but only use 1 in the block
        blockchain.Mempool.Add(new Transaction());
        blockchain.Mempool.Add(new Transaction());

        var tx = new Transaction(
            txInputs: new List<TransactionInput> { new TransactionInput() }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx }
        );

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_GetBlock_ShouldGetBlock()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var block = blockchain.GetBlock(blockchain.GetLastBlock().Hash);

        // Assert
        Assert.NotNull(block);
    }

    [Fact]
    public void BlockchainTests_AddBlock_ShouldNotAddBlockInvalidIndex()
    {
        // Arrange
        var blockchain = new EF.Blockchain.Domain.Blockchain(_loki.PublicKey);
        blockchain.Mempool.Add(new Transaction()); // simulate a pending tx

        var feeTx = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput>
            {
            new TransactionOutput(toAddress: _loki.PublicKey, amount: 1)
            }
        );

        var block = new Block(
            index: -1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { feeTx }
        );

        // Use reflection to change private/internal state
        typeof(Block)
            .GetProperty(nameof(Block.Hash))!
            .SetValue(
                block, block.GetHash());

        // Act
        var result = blockchain.AddBlock(block);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void BlockchainTests_GetNextBlock_ShouldGetNextBlockInfo()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

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
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var info = blockchain.GetNextBlock();

        // Assert
        Assert.Null(info);
    }

    [Fact]
    public void BlockchainTests_GetBalance_ShouldGetBalance()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var balance = blockchain.GetBalance(_loki.PublicKey);

        // Assert
        balance.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BlockchainTests_GetBalance_ShouldGetZeroBalance()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);

        // Act
        var balance = blockchain.GetBalance(_thor.PublicKey);

        // Assert
        balance.Should().Be(0);
    }

    [Fact]
    public void BlockchainTests_GetUtxo_ShouldGetUtxo()
    {
        // Arrange
        var blockchain = new Domain.Blockchain(_loki.PublicKey);
        var baseTx = blockchain.Blocks[0].Transactions[0];

        var input = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: baseTx.Hash
        );
        input.Sign(_loki.PrivateKey);

        var tx = new Transaction(
            txInputs: new List<TransactionInput> { input },
            txOutputs: new List<TransactionOutput>
            {
            new TransactionOutput("abc", 5),
            new TransactionOutput(_loki.PublicKey, 4)
            }
        );

        var block = new Block(
            index: 1,
            previousHash: blockchain.GetLastBlock().Hash,
            transactions: new List<Transaction> { tx }
        );

        blockchain.Blocks.Add(block);

        // Act
        var utxos = blockchain.GetUtxo(_loki.PublicKey);

        // Assert
        utxos.Should().NotBeEmpty();
    }
}
