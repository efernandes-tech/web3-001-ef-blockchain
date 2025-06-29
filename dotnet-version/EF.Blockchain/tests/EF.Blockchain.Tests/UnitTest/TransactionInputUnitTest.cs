using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionInputUnitTest
{
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
}
