using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

/// <summary>
/// Block class
/// </summary>
public class Block
{
    public int Index { get; private set; }
    public long Timestamp { get; private set; }
    public string Hash { get; private set; }
    public string PreviousHash { get; private set; }
    public List<Transaction> Transactions { get; private set; } = new();
    public int Nonce { get; private set; }
    public string Miner { get; private set; }

    /// <summary>
    /// Creates a new Block
    /// </summary>
    /// <param name="index">The block index in blockchain</param>
    /// <param name="previousHash">The previous block hash</param>
    /// <param name="transactions">The block transactions</param>
    /// <param name="timestamp">The block timestamp</param>
    /// <param name="hash">The block hash</param>
    /// <param name="nonce">The block nonce</param>
    /// <param name="miner">The block miner</param>
    public Block(int? index = null,
        string? previousHash = null,
        List<Transaction>? transactions = null,
        long? timestamp = null,
        string? hash = null,
        int? nonce = null,
        string? miner = null)
    {
        Index = index ?? 0;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PreviousHash = previousHash ?? string.Empty;
        Transactions = transactions ?? new();
        Nonce = nonce ?? 0;
        Miner = miner ?? string.Empty;
        Hash = string.IsNullOrEmpty(hash) ? GetHash() : hash;
    }

    /// <summary>
    /// Generates the hash for the block based on its data.
    /// </summary>
    /// <returns>The SHA-256 hash string.</returns>
    public string GetHash()
    {
        var txs = Transactions != null && Transactions.Any()
            ? Transactions.Select(tx => tx.Hash).Aggregate((a, b) => a + b)
            : "";

        return ComputeHash(Index, Timestamp, txs, PreviousHash, Nonce, Miner);
    }

    public static string ComputeHash(int index,
        long timestamp,
        string txs,
        string previousHash,
        int nonce = 0,
        string? miner = null)
    {
        var rawData = $"{index}{txs}{timestamp}{previousHash}{nonce}{miner}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawData);
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }

    /// <summary>
    /// Generates a new valid hash for this block with the specified difficulty
    /// </summary>
    /// <param name="difficulty">The blockchain current difficulty</param>
    /// <param name="miner">The miner wallet address</param>
    public void Mine(int difficulty, string miner)
    {
        Miner = miner;
        var prefix = new string('0', difficulty);

        do
        {
            Nonce++;
            Hash = GetHash();
        }
        while (!Hash.StartsWith(prefix));
    }

    /// <summary>
    /// Validates the block
    /// </summary>
    /// /// <param name="previousHash">The previous block hash</param>
    /// /// <param name="previousIndex">The previous block index</param>
    /// /// <param name="difficulty">The blockchain current difficulty</param>
    /// <returns><c>Validation</c> if the block is valid</returns>
    public Validation IsValid(string previousHash, int previousIndex, int difficulty)
    {
        if (Transactions != null && Transactions.Any())
        {
            var feeTxs = Transactions
                .Where(tx => tx.Type == TransactionType.FEE)
                .ToList();

            if (!feeTxs.Any())
                return new Validation(false, "No fee tx");

            if (feeTxs.Count > 1)
                return new Validation(false, "Too many fees");

            if (!feeTxs[0].TxOutputs.Any(txo => txo.ToAddress == Miner))
                return new Validation(false, "Invalid fee tx: different from miner");

            var validations = Transactions
                .Select(tx => tx.IsValid())
                .Where(v => !v.Success)
                .Select(v => v.Message)
                .ToList();

            if (validations.Any())
            {
                var errorMsg = string.Join(" ", validations);
                return new Validation(false, "Invalid block due to invalid tx: " + errorMsg);
            }
        }

        if (previousIndex != Index - 1)
            return new Validation(false, "Invalid index");

        if (Timestamp < 1)
            return new Validation(false, "Invalid timestamp");

        if (PreviousHash != previousHash)
            return new Validation(false, "Invalid previous hash");

        if (Nonce < 1 || string.IsNullOrEmpty(Miner))
            return new Validation(false, "No mined");

        var prefix = new string('0', difficulty);
        if (Hash != GetHash() || !Hash.StartsWith(prefix))
            return new Validation(false, "Invalid hash");

        return new Validation();
    }

    public static Block FromBlockInfo(BlockInfo blockInfo)
    {
        return new Block
        {
            Index = blockInfo.Index,
            PreviousHash = blockInfo.PreviousHash,
            Transactions = blockInfo.Transactions
        };
    }
}
