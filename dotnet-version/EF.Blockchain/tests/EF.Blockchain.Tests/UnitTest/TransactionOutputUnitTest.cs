using EF.Blockchain.Domain;
using FluentAssertions;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionOutputUnitTest
{
    private readonly Wallet _loki;
    private readonly Wallet _thor;

    public TransactionOutputUnitTest()
    {
        _loki = new Wallet();
        _thor = new Wallet();
    }

    [Fact]
    public void TransactionOutputTests_IsValid_ShouldBeValid()
    {
        // Arrange
        var txOutput = new TransactionOutput(
            amount: 10,
            toAddress: _loki.PublicKey,
            tx: "abc"
        );

        // Act
        var valid = txOutput.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionOutputTests_IsValid_ShouldNotBeValidDefault()
    {
        // Arrange
        var txOutput = new TransactionOutput();

        // Act
        var result = txOutput.IsValid();

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void TransactionOutputTests_IsValid_ShouldNotBeValid()
    {
        // Arrange
        var txOutput = new TransactionOutput(
            toAddress: _loki.PublicKey,
            amount: -10,
            tx: "abc"
        );

        // Act
        var result = txOutput.IsValid();

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void TransactionOutputTests_GetHash_ShouldGetHash()
    {
        // Arrange
        var txOutput = new TransactionOutput(
            toAddress: _loki.PublicKey,
            amount: 10,
            tx: "abc"
        );

        // Act
        var hash = txOutput.GetHash();

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
    }
}
