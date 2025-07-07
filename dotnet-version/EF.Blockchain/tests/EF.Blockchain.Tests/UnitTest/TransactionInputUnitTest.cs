using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionInputUnitTest
{
    private const string _exampleTx = "8eba6c75bbd12d9e21f657b76726312aad08f2d3a10aee52d2b1017e6248c186";

    private readonly Wallet _loki;
    private readonly Wallet _thor;

    public TransactionInputUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "abc"
        );

        txInput.Sign(_loki.PrivateKey);

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.True(valid.Success);
        Assert.Equal("", valid.Message);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidDefaults()
    {
        // Arrange
        var txInput = new TransactionInput();

        txInput.Sign(_loki.PrivateKey);

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidEmptySignature()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "abc"
        );

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidNegativeAmount()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: -10,
            previousTx: "abc"
        );

        txInput.Sign(_loki.PrivateKey);

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidInvalidSignature()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10,
            previousTx: "abc"
        );

        txInput.Sign(_thor.PrivateKey); // Wrong key used

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidInvalidPreviousTx()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10
        );
        txInput.Sign(_loki.PrivateKey);

        // Act
        var result = txInput.IsValid();

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldCreateFromTxo()
    {
        // Arrange
        var txo = new TransactionOutput(_loki.PublicKey, 10, _exampleTx);

        var txi = TransactionInput.FromTxo(txo);
        txi.Sign(_loki.PrivateKey);

        // Force invalid hash with reflection
        typeof(TransactionInput)
            .GetProperty(nameof(TransactionInput.Amount))!
            .SetValue(txi, 11);

        // Act
        var result = txi.IsValid();

        // Assert
        result.Success.Should().BeFalse();
    }

    // ########## ########## ########## ########## ##########

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidErrorVerifyingSing()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: "abc",
            amount: 10,
            previousTx: "def"
        );
        txInput.Sign(_loki.PrivateKey);

        // Act
        var result = txInput.IsValid();

        // Assert
        result.Success.Should().BeFalse();
        Assert.Contains("Error verifying signature", result.Message);
    }
}
