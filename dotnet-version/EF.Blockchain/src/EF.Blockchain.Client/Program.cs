using EF.Blockchain.Domain;
using Flurl.Http;
using Microsoft.Extensions.Configuration;

// Load configuration from appsettings.json
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Access values from configuration
var blockchainServer = config["Blockchain:Server"];
var minerWalletPublicKey = config["Blockchain:MinerWallet:PublicKey"];
var minerWalletPrivateKey = config["Blockchain:MinerWallet:PrivateKey"];

int totalMined = 0;

Console.WriteLine("Starting miner...");

Console.WriteLine("Logged as " + minerWalletPublicKey);

while (true)
{
    try
    {
        Console.WriteLine("Getting next block info...");

        // Uses Flurl.Http to send an HTTP GET request
        var blockInfo = await $"{blockchainServer}/blocks/next"
            .GetJsonAsync<BlockInfo>();

        if (blockInfo == null)
        {
            Console.WriteLine("No tx found. Waiting...");
            await Task.Delay(5000);
            continue;
        }

        var newBlock = Block.FromBlockInfo(blockInfo);

        // TODO: Add reward transaction here if needed

        Console.WriteLine($"Start mining block #{blockInfo.Index}...");
        newBlock.Mine(blockInfo.Difficulty, minerWalletPublicKey);

        Console.WriteLine("Block mined! Sending to blockchain...");

        await $"{blockchainServer}/blocks/"
            .PostJsonAsync(newBlock);

        Console.WriteLine("Block sent and accepted!");
        totalMined++;
        Console.WriteLine($"Total mined blocks: {totalMined}");
    }
    catch (FlurlHttpException ex)
    {
        var message = ex.Call?.Response != null
            ? await ex.GetResponseStringAsync()
            : ex.Message;

        Console.WriteLine("Error: " + message);
    }

    await Task.Delay(1000);
}
