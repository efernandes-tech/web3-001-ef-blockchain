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

        var tx = new Transaction(txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput> { new TransactionOutput(toAddress: TransactionMockFactory.MockedPublicKeyTo, amount: 1000) });

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
            fromAddress: TransactionMockFactory.MockedPublicKey,
            amount: 1000
        );
        txInput.Sign(TransactionMockFactory.MockedPrivateKey);
        var tx = new Transaction(
            type: TransactionType.FEE,
            txInputs: new List<TransactionInput> { txInput },
            txOutputs: new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: TransactionMockFactory.MockedPublicKeyTo,
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
