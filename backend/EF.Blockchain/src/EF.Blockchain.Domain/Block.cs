using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

/// <summary>
/// Represents a block in the blockchain. Each block contains a list of transactions and metadata like index, timestamp, hash, etc.
/// </summary>
public class Block
{
    /// <summary>
    /// The index of the block in the blockchain.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The Unix timestamp when the block was created.
    /// </summary>
    public long Timestamp { get; private set; }

    /// <summary>
    /// The SHA-256 hash of the block.
    /// </summary>
    public string Hash { get; private set; }

    /// <summary>
    /// The hash of the previous block in the chain.
    /// </summary>
    public string PreviousHash { get; private set; }

    /// <summary>
    /// List of transactions included in this block.
    /// </summary>
    public List<Transaction> Transactions { get; private set; } = new();

    /// <summary>
    /// The nonce value used for proof-of-work (mining).
    /// </summary>
    public int Nonce { get; private set; }

    /// <summary>
    /// The wallet address of the miner who mined this block.
    /// </summary>
    public string Miner { get; private set; }

    /// <summary>
    /// Creates a new instance of a block.
    /// </summary>
    /// <param name="index">The index of the block in the chain.</param>
    /// <param name="previousHash">Hash of the previous block.</param>
    /// <param name="transactions">List of transactions included.</param>
    /// <param name="timestamp">Timestamp of block creation.</param>
    /// <param name="hash">Optional predefined hash.</param>
    /// <param name="nonce">Nonce value used for mining.</param>
    /// <param name="miner">Miner's wallet address.</param>
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
    /// Generates the SHA-256 hash for the block based on its current data.
    /// </summary>
    /// <returns>The generated hash string.</returns>
    public string GetHash()
    {
        var txs = Transactions != null && Transactions.Any()
            ? Transactions.Select(tx => tx.Hash).Aggregate((a, b) => a + b)
            : "";

        return ComputeHash(Index, Timestamp, txs, PreviousHash, Nonce, Miner);
    }

    /// <summary>
    /// Computes a SHA-256 hash based on provided parameters.
    /// </summary>
    /// <param name="index">Block index.</param>
    /// <param name="timestamp">Block timestamp.</param>
    /// <param name="txs">Concatenated transaction hashes.</param>
    /// <param name="previousHash">Hash of the previous block.</param>
    /// <param name="nonce">Nonce used in mining.</param>
    /// <param name="miner">Miner's wallet address.</param>
    /// <returns>Computed SHA-256 hash.</returns>
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
        return Convert.ToHexString(sha256.ComputeHash(bytes)).ToLower();
    }

    /// <summary>
    /// Mines the block by incrementing the nonce until the hash starts with the required number of zeros.
    /// </summary>
    /// <param name="difficulty">Number of leading zeros required in the hash.</param>
    /// <param name="miner">Miner's wallet address.</param>
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
    /// Validates this block using previous block data and difficulty.
    /// </summary>
    /// <param name="previousHash">Hash of the previous block.</param>
    /// <param name="previousIndex">Index of the previous block.</param>
    /// <param name="difficulty">Current difficulty level.</param>
    /// <param name="feePerTx">Expected fee per transaction.</param>
    /// <returns>A <see cref="Validation"/> result indicating if the block is valid.</returns>
    public Validation IsValid(string previousHash, int previousIndex, int difficulty, int feePerTx)
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

            var totalFees = feePerTx * Transactions.Count(tx => tx.Type != TransactionType.FEE);

            var validations = Transactions
                .Select(tx => tx.IsValid(difficulty, totalFees))
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

    /// <summary>
    /// Creates a new block from a given <see cref="BlockInfo"/> object.
    /// </summary>
    /// <param name="blockInfo">The block info used to create a block.</param>
    /// <returns>A new <see cref="Block"/> instance.</returns>
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
