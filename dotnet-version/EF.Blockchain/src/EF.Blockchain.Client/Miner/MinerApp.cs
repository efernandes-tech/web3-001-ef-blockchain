using EF.Blockchain.Domain;
using Flurl.Http;

namespace EF.Blockchain.Client.Miner;

public class MinerApp
{
    private readonly string _blockchainServer;
    private readonly Domain.Wallet _minerWallet;
    private int _totalMined = 0;

    public MinerApp(string blockchainServer, string privateKey)
    {
        _blockchainServer = blockchainServer;
        _minerWallet = new Domain.Wallet(privateKey);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Starting miner...");
        Console.WriteLine("Logged as " + _minerWallet.PublicKey);

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

                // Add reward transaction (FEE)
                var rewardTransaction = new Transaction(
                    type: TransactionType.FEE,
                    to: _minerWallet.PublicKey
                );
                newBlock.Transactions.Add(rewardTransaction);

                Console.WriteLine($"Start mining block #{blockInfo.Index}...");
                newBlock.Mine(blockInfo.Difficulty, _minerWallet.PublicKey);

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
