using EF.Blockchain.Client.Miner;
using EF.Blockchain.Client.Wallet;

Console.WriteLine("Select App to run:");
Console.WriteLine("1 - Miner");
Console.WriteLine("2 - Wallet");

var option = Console.ReadLine();

switch (option)
{
    case "1":
        Console.WriteLine("Starting Miner...");
        var miner = MinerAppFactory.Create();
        await miner.RunAsync();
        break;

    case "2":
        Console.WriteLine("Starting Wallet...");
        var wallet = WalletAppFactory.Create();
        wallet.ShowInfo();
        break;

    default:
        Console.WriteLine("Invalid option");
        break;
}
