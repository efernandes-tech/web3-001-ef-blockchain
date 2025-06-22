using Microsoft.Extensions.Configuration;

namespace EF.Blockchain.Client.Wallet;

public static class WalletAppFactory
{
    public static WalletApp Create()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var privateKey = config["Blockchain:MinerWallet:PrivateKey"]!;

        return new WalletApp();
    }
}
