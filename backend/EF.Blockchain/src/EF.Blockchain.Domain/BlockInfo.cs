namespace EF.Blockchain.Domain;

/// <summary>
/// Represents the metadata required to create or mine the next block in the blockchain.
/// </summary>
public class BlockInfo
{
    /// <summary>
    /// Index of the next block to be mined.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Hash of the last confirmed block in the chain.
    /// </summary>
    public string PreviousHash { get; private set; } = string.Empty;

    /// <summary>
    /// Current difficulty level for mining.
    /// </summary>
    public int Difficulty { get; private set; }

    /// <summary>
    /// Maximum allowed difficulty in the network.
    /// </summary>
    public int MaxDifficulty { get; private set; }

    /// <summary>
    /// Fee applied per transaction included in the block.
    /// </summary>
    public int FeePerTx { get; private set; }

    /// <summary>
    /// Transactions selected from the mempool for inclusion in the block.
    /// </summary>
    public List<Transaction> Transactions { get; private set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockInfo"/> class.
    /// </summary>
    /// <param name="index">Index of the block.</param>
    /// <param name="previousHash">Hash of the previous block.</param>
    /// <param name="difficulty">Current mining difficulty.</param>
    /// <param name="maxDifficulty">Maximum difficulty allowed.</param>
    /// <param name="feePerTx">Fee per transaction.</param>
    /// <param name="transactions">Transactions included in the block.</param>
    public BlockInfo(int index, string previousHash, int difficulty, int maxDifficulty, int feePerTx, List<Transaction> transactions)
    {
        Index = index;
        PreviousHash = previousHash;
        Difficulty = difficulty;
        MaxDifficulty = maxDifficulty;
        FeePerTx = feePerTx;
        Transactions = transactions;
    }
}
