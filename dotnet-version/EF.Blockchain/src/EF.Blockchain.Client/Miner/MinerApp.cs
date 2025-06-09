using EF.Blockchain.Domain;
using Flurl.Http;

namespace EF.Blockchain.Client.Miner;

public class MinerApp
{
    private readonly string _blockchainServer;
    private readonly string _publicKey;
    private int _totalMined = 0;

    public MinerApp(string blockchainServer, string publicKey)
    {
        _blockchainServer = blockchainServer;
        _publicKey = publicKey;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Starting miner...");
        Console.WriteLine("Logged as " + _publicKey);

        while (true)
        {
            try
            {
                Console.WriteLine("Getting next block info...");

                // Uses Flurl.Http to send an HTTP GET request
                var blockInfo = await $"{_blockchainServer}/blocks/next"
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
                newBlock.Mine(blockInfo.Difficulty, _publicKey);

                Console.WriteLine("Block mined! Sending to blockchain...");

                await $"{_blockchainServer}/blocks/"
                    .PostJsonAsync(newBlock);

                Console.WriteLine("Block sent and accepted!");
                _totalMined++;
                Console.WriteLine($"Total mined blocks: {_totalMined}");
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
    }
}
