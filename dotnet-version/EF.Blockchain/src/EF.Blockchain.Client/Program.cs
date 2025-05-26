using EF.Blockchain.Domain;
using Flurl.Http;

const string BLOCKCHAIN_SERVER = "http://localhost:3000";
const string MinerPublicKey = "efernandes";
const string MinerPrivateKey = "123456";
int totalMined = 0;

Console.WriteLine("Starting miner...");

while (true)
{
    try
    {
        Console.WriteLine("Getting next block info...");

        // Uses Flurl.Http to send an HTTP GET request
        var blockInfo = await $"{BLOCKCHAIN_SERVER}/blocks/next"
            .GetJsonAsync<BlockInfo>();

        var newBlock = Block.FromBlockInfo(blockInfo);

        // TODO: Add reward transaction here if needed

        Console.WriteLine($"Start mining block #{blockInfo.Index}...");
        newBlock.Mine(blockInfo.Difficulty, MinerPublicKey);

        Console.WriteLine("Block mined! Sending to blockchain...");

        await $"{BLOCKCHAIN_SERVER}/blocks/"
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
