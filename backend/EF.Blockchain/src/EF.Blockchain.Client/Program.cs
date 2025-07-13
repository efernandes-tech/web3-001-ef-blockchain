using EF.Blockchain.Client.Miner;
using EF.Blockchain.Client.Wallet;

if (args.Length > 0)
{
    switch (args[0].ToLower())
    {
        case "miner":
        case "1":
            Console.WriteLine("Starting Miner...");
            var minerApp = MinerAppFactory.Create();
            await minerApp.RunAsync();
            return;

        case "wallet":
        case "2":
            Console.WriteLine("Starting Wallet...");
            var walletApp = WalletAppFactory.Create();
            await walletApp.RunAsync();
            return;

        default:
            Console.WriteLine("Invalid argument. Use: miner or wallet");
            return;
    }
}

Console.WriteLine("Select App to run:");
Console.WriteLine("1 - Miner");
Console.WriteLine("2 - Wallet");

var option = Console.ReadLine();

switch (option)
{
    case "1":
        Console.WriteLine("Starting Miner...");
        var minerApp = MinerAppFactory.Create();
        await minerApp.RunAsync();
        break;

    case "2":
        Console.WriteLine("Starting Wallet...");
        var walletApp = WalletAppFactory.Create();
        await walletApp.RunAsync();
        break;

    default:
        Console.WriteLine("Invalid option");
        break;
}
