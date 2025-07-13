using EF.Blockchain.Domain;
using Flurl.Http;

namespace EF.Blockchain.Client.Miner;

public class MinerApp
{
    private readonly string _blockchainServer;
    private readonly Domain.Wallet _minerWallet;
    private int _totalMined = 0;
    private readonly string _logFilePath;
    private readonly long _maxLogSizeBytes = 2 * 1024 * 1024; // 2MB

    public MinerApp(string blockchainServer, string privateKey)
    {
        _blockchainServer = blockchainServer;
        _minerWallet = new Domain.Wallet(privateKey);
        _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "miner-logs.txt");
    }

    public async Task RunAsync()
    {
        LogMessage("Starting miner...");
        LogMessage("Logged as " + _minerWallet.PublicKey);

        while (true)
        {
            try
            {
                LogMessage("Getting next block info...");

                // Uses Flurl.Http to send an HTTP GET request
                var blockInfo = await $"{_blockchainServer}/blocks/next"
                    .GetJsonAsync<BlockInfo>();

                if (blockInfo == null)
                {
                    LogMessage("No tx found. Waiting...");
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

                LogMessage($"Start mining block #{blockInfo.Index}...");

                newBlock.Mine(blockInfo.Difficulty, _minerWallet.PublicKey);

                LogMessage("Block mined! Sending to blockchain...");

                await $"{_blockchainServer}/blocks/"
                    .PostJsonAsync(newBlock);

                LogMessage("Block sent and accepted!");
                _totalMined++;
                LogMessage($"Total mined blocks: {_totalMined}");
            }
            catch (FlurlHttpException ex)
            {
                var message = ex.Call?.Response != null
                    ? await ex.GetResponseStringAsync()
                    : ex.Message;

                LogMessage("Error: " + message);
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
            LogMessage("Low fees. Awaiting next block.");
            return null;
        }

        amount += fees;

        var rewardOutput = new TransactionOutput(
            toAddress: _minerWallet.PublicKey,
            amount: amount
        );

        return Transaction.FromReward(rewardOutput);
    }

    private void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}";

        // Write to console
        Console.WriteLine(message);

        // Write to file
        try
        {
            // Check file size and rotate if necessary
            if (File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > _maxLogSizeBytes)
                {
                    RotateLogFile();
                }
            }

            // Prepend to file (add to top)
            var existingContent = File.Exists(_logFilePath) ? File.ReadAllText(_logFilePath) : "";
            File.WriteAllText(_logFilePath, logEntry + Environment.NewLine + existingContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    private void RotateLogFile()
    {
        try
        {
            var backupPath = _logFilePath.Replace(".txt", $"_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt");
            File.Move(_logFilePath, backupPath);

            // Keep only last 5 backup files
            var logDirectory = Path.GetDirectoryName(_logFilePath);
            var backupFiles = Directory.GetFiles(logDirectory, "miner-logs_*.txt")
                .OrderByDescending(f => f)
                .Skip(5);

            foreach (var oldFile in backupFiles)
            {
                File.Delete(oldFile);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to rotate log file: {ex.Message}");
        }
    }
}
