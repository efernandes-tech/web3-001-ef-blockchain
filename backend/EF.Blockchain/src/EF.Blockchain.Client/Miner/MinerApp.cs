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

                var rewardTx = GetRewardTx(blockInfo, newBlock);
                if (rewardTx == null)
                {
                    await Task.Delay(5000);
                    continue;
                }

                newBlock.Transactions.Add(rewardTx);

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

    private Transaction? GetRewardTx(BlockInfo blockInfo, Block nextBlock)
    {
        var amount = 0;

        if (blockInfo.Difficulty <= blockInfo.MaxDifficulty)
            amount += Domain.Blockchain.GetRewardAmount(blockInfo.Difficulty);

        var fees = nextBlock.Transactions.Sum(tx => tx.GetFee());
        var feeCheck = nextBlock.Transactions.Count * blockInfo.FeePerTx;

        if (fees < feeCheck)
        {
            Console.WriteLine("Low fees. Awaiting next block.");
            return null;
        }

        amount += fees;

        var rewardOutput = new TransactionOutput(
            toAddress: _minerWallet.PublicKey,
            amount: amount
        );

        return Transaction.FromReward(rewardOutput);
    }
}
