using EF.Blockchain.Domain;

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
            amount: 10
        );

        txInput.Sign(_loki.PrivateKey);

        // Act
        var valid = txInput.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidDefaults()
    {
        var txInput = new TransactionInput();

        txInput.Sign(_loki.PrivateKey);

        var valid = txInput.IsValid();

        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidEmptySignature()
    {
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10
        );

        var valid = txInput.IsValid();

        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidNegativeAmount()
    {
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: -10
        );

        txInput.Sign(_loki.PrivateKey);

        var valid = txInput.IsValid();

        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidInvalidSignature()
    {
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10
        );

        txInput.Sign(_thor.PrivateKey); // Wrong key used

        var valid = txInput.IsValid();

        Assert.False(valid.Success);
    }

    [Fact]
    public void TransactionInputTests_IsValid_ShouldNotBeValidErrorVerifyingSignature()
    {
        var txInput = new TransactionInput(
            fromAddress: _loki.PublicKey,
            amount: 10
        );

        txInput.Sign(_thor.PrivateKey); // Wrong key used
        // Force invalid signature with reflection
        typeof(TransactionInput)
            .GetProperty(nameof(TransactionInput.Signature))!
            .SetValue(txInput, "abc");

        var valid = txInput.IsValid();

        Assert.False(valid.Success);
    }
}
