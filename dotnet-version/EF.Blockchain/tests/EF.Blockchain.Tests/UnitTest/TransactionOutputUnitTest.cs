using EF.Blockchain.Domain;

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
}
