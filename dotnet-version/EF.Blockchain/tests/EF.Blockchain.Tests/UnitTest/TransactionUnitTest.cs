using EF.Blockchain.Domain;
using EF.Blockchain.Tests.Mocks;

namespace EF.Blockchain.Tests.UnitTest;

public class TransactionUnitTest
{
    [Fact]
    public void TransactionTests_IsValid_ShouldBeValidRegularDefault()
    {
        // Arrange
        var txInput = new TransactionInput(
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000
        );
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);

        var tx = new Transaction(txInput: txInput, to: TransactionMockFactory.MockedPublicKeyTo);

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
            txInput: new TransactionInput()
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
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000
        );
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);
        var tx = new Transaction(
            type: TransactionType.FEE,
            txInput: txInput,
            to: TransactionMockFactory.MockedPublicKeyTo
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
            txInput: txInput,
            to: "carteiraTo"
        );

        // Act
        var valid = tx.IsValid();

        // Assert
        Assert.False(valid.Success);
    }
}
