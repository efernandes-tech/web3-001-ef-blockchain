using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionUnitTest
{
    [Fact]
    public void TransactionTests_IsValid_ShouldBeValidRegularDefault()
    {
        // Arrange
        var tx = new Transaction(data: "tx");

        // Act
        var valid = tx.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidHash()
    {
        // Arrange
        var tx = new Transaction(
            type: TransactionType.REGULAR,
            timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data: "tx"
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
        var tx = new Transaction(
            type: TransactionType.FEE,
            data: "tx"
        );

        // Act
        var valid = tx.IsValid();

        // Assert
        Assert.True(valid.Success);
    }

    [Fact]
    public void TransactionTests_IsValid_ShouldNotBeValidInvalidData()
    {
        // Arrange
        var tx = new Transaction(data: "");

        // Act
        var valid = tx.IsValid();

        // Assert
        Assert.False(valid.Success);
    }
}
