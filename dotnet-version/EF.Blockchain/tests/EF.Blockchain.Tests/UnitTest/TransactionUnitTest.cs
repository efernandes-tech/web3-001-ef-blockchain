using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionUnitTest
{
    private const int _exampleDifficulty = 1;
    private const int _exampleFee = 1;
    private const string _exampleTx = "8eba6c75bbd12d9e21f657b76726312aad08f2d3a10aee52d2b1017e6248c186";

    private readonly Wallet _loki;
    private readonly Wallet _thor;

    public TransactionUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldBeValidRegularDefault()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 1000,
            previousTx: "previousTxHash"
        );
        txInput.Sign(_loki.PrivateKey);

        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: _loki.PublicKey, amount: 1000) });

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidTxoHashDiffTxHash()
    {
        // Arrange
        var txInput = new TransactionInput(_loki.PublicKey, 10, "abc", previousTx: "xyz");
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(_loki.PublicKey, 10);
        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        txOutput.SetTx("mismatch");

        // Act
        var result = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        result.Success.Should().BeFalse();
        Assert.Contains("Invalid TXO reference hash", result.Message);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInputsLessOutputs()
    {
        // Arrange
        var txInput = new TransactionInput(_loki.PublicKey, 1, "abc", "xyz");
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(_loki.PublicKey, 2);
        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        // Act
        var result = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        result.Success.Should().BeFalse();
        Assert.Contains("Invalid tx: input amounts must be equals or greater than outputs amounts", result.Message);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidHash()
    {
        // Arrange
        var tx = new Transaction(
            type: TransactionType.REGULAR,
            timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            txInputs: new List<TransactionInput> { new TransactionInput() }
        );

        // Force invalid hash with reflection
        typeof(Transaction)
            .GetProperty(nameof(Transaction.Hash))!
            .SetValue(tx, "abc");

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldBeValidFee()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 1,
            previousTx: "previousTxHash"
        );

        txInput.Sign(_loki.PrivateKey);

        var tx = new Transaction(
            type: TransactionType.FEE,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: _loki.PublicKey,
                    amount: 1
                )
            }
        );

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidTo()
    {
        // Arrange
        var tx = new Transaction();

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidTxInput()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: "carteiraFrom",
            amount: -10,
            signature: "abc" // random invalid signature
        );

        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> {
                new TransactionOutput(
                    toAddress: "carteiraTo",
                    amount: 1000
                )
            }
        );

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionTests_GetFee_ShouldGetFee()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 11,
            previousTx: _exampleTx
        );
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(
            toAddress: _thor.PublicKey,
            amount: 10
        );

        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        // Act
        var fee = tx.GetFee();

        // Assert
        fee.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TransactionTests_GetFee_ShouldGetZeroFee()
    {
        // Arrange
        var tx = new Transaction(); // No inputs

        // Act
        var fee = tx.GetFee();

        // Assert
        fee.Should().Be(0);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldCreateFromReward()
    {
        // Arrange
        var txo = new TransactionOutput(_loki.PublicKey, 10);
        var tx = Transaction.FromReward(txo);

        // Act
        var valid = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        valid.Success.Should().BeTrue();
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidFeeExcess()
    {
        // Arrange
        var txo = new TransactionOutput(
            toAddress: _thor.PublicKey,
            amount: int.MaxValue
        );

        var tx = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> { txo }
        );

        // Act
        var result = tx.IsValid(_exampleDifficulty, _exampleFee);

        // Assert
        result.Success.Should().BeFalse();
    }
}
