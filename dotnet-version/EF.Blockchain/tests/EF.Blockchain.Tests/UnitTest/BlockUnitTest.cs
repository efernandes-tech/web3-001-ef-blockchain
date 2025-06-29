using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.UnitTest;

public class BlockUnitTest
{
    private readonly int ExampleDifficulty = 1;
    private readonly Wallet _loki;
    private readonly Wallet _thor;
    private Block Genesis;

    public BlockUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();

        var genesisTx = new Transaction(
            type: TransactionType.FEE
        );

        Genesis = new Block(
            transactions: new List<Transaction> { genesisTx });
    }

    [Fact]
    public void BlockTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 10
                )
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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValidFeeForOther()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);
        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 10
                )
            }
        );

        var transactionFee = new Transaction(
           type: TransactionType.FEE,
           txOutputs: new List<TransactionOutput>
           {
                new TransactionOutput(
                     toAddress: _thor.PublicKey,
                     amount: 1
                )
           }
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var transaction = new Transaction(
           type: TransactionType.REGULAR
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
           type: TransactionType.REGULAR,
           txInputs: new List<TransactionInput> { txInput },
           txOutputs: new List<TransactionOutput> {
               new TransactionOutput(
                   toAddress: _loki.PublicKey,
                   amount: 10
               )
           }
        );

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
            PreviousHash = Genesis.GetHash(),
            Transactions = new List<Transaction> { transaction, transactionFee },
            Difficulty = ExampleDifficulty,
            FeePerTx = 1,
            MaxDifficulty = 62,
        };

        // Act
        var block = Block.FromBlockInfo(blockInfo);
        block.Mine(ExampleDifficulty, _loki.PublicKey);
        var validation = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.True(validation.Success);
    }

    [Fact]
    public void BlockTests_IsValid_ShouldNotBeValid2Fee()
    {
        // Arrange
        var tx1 = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput()
            }
        );
        var tx2 = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> { new TransactionOutput() }
        );

        var block = new Block(
            index: 1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { tx1, tx2 }
        );
        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { invalidTx, transactionFee }
        );

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);
        var transaction = new Transaction(
           type: TransactionType.REGULAR,
           txInputs: new List<TransactionInput>
           {
               txInput
           },
           txOutputs: new List<TransactionOutput>
           {
               new TransactionOutput(
                   toAddress: _loki.PublicKey,
                   amount: 10
               )
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
            previousHash: "abc",
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);
        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 10
                )
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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);
        var transaction = new Transaction(
           type: TransactionType.REGULAR,
           txInputs: new List<TransactionInput> { txInput },
           txOutputs: new List<TransactionOutput>
              {
                 new TransactionOutput(
                        toAddress: _loki.PublicKey,
                        amount: 10
                 )
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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);
        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 10
                )
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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee },
            nonce: 0,
            miner: _loki.PublicKey);

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
        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                new TransactionInput(
                    amount: -1
                )
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
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

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
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);

        var transaction = new Transaction(
            type: TransactionType.REGULAR,
            txInputs: new List<TransactionInput>
            {
                txInput
            },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 10
                )
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
            index: -1,
            previousHash: Genesis.Hash,
            transactions: new List<Transaction> { transaction, transactionFee });

        block.Mine(ExampleDifficulty, _loki.PublicKey);

        // Act
        var valid = block.IsValid(Genesis.Hash, Genesis.Index, ExampleDifficulty);

        // Assert
        Assert.False(valid.Success);
        Assert.Contains("Invalid index", valid.Message);
    }
}
