using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.UnitTest;

public class WalletUnitTest
{
    private readonly Wallet _loki;
    private const string ExampleWIF = "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ"; // Satoshi WIF

    public WalletUnitTest()
    {
        _loki = new Wallet();
    }

    [Fact]
    public void WalletTests_Init_ShouldGenerateWallet()
    {
        // Arrange & Act
        var wallet = new Wallet();

        // Assert
        Assert.False(string.IsNullOrEmpty(wallet.PrivateKey));
        Assert.False(string.IsNullOrEmpty(wallet.PublicKey));
    }

    [Fact]
    public void WalletTests_InitPK_ShouldRecoverWalletPK()
    {
        // Arrange
        var recovered = new Wallet(_loki.PrivateKey);

        // Assert
        Assert.Equal(_loki.PublicKey, recovered.PublicKey);
    }

    [Fact]
    public void WalletTests_InitWIF_ShouldRecoverWalletWIF()
    {
        // Arrange
        var wallet = new Wallet(ExampleWIF);

        // Assert
        Assert.False(string.IsNullOrEmpty(wallet.PrivateKey));
        Assert.False(string.IsNullOrEmpty(wallet.PublicKey));
    }
}
