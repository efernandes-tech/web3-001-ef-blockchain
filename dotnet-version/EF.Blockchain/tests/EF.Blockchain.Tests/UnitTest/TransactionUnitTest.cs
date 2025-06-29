using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionUnitTest
{
    private readonly Wallet _loki;

    public TransactionUnitTest()
    {
        _loki = new Wallet();
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
        var valid = tx.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidTxoHashDiffTxHash()
    {
        // Arrange
        var txInput = new TransactionInput(_loki.PublicKey, 10, "abc");
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(_loki.PublicKey, 10);
        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        txOutput.SetTx("mismatch");

        // Act
        var result = tx.IsValid();

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInputsLessOutputs()
    {
        // Arrange
        var txInput = new TransactionInput(_loki.PublicKey, 1, "abc");
        txInput.Sign(_loki.PrivateKey);

        var txOutput = new TransactionOutput(_loki.PublicKey, 2);
        var tx = new Transaction(
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { txOutput }
        );

        // Act
        var result = tx.IsValid();

        // Assert
        result.Success.Should().BeFalse();
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
        var valid = tx.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldBeValidFee()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 1000,
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
                    amount: 1000
                )
            }
        );

        // Act
        var valid = tx.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidTo()
    {
        // Arrange
        var tx = new Transaction();

        // Act
        var valid = tx.IsValid();

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
        var valid = tx.IsValid();

        // Assert
        Assert.False(valid.Success);
    }
}
