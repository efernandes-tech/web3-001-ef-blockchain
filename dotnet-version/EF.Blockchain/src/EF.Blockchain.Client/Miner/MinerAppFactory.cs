using Microsoft.Extensions.Configuration;

namespace EF.Blockchain.Client.Miner;

public static class MinerAppFactory
{
    public static MinerApp Create()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var server = config["Blockchain:Server"]!;
        var publicKey = config["Blockchain:MinerWallet:PublicKey"]!;

        return new MinerApp(server, publicKey);
    }
}
