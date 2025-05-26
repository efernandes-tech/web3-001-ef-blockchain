namespace EF.Blockchain.Domain;

/// <summary>
/// Blockchain class
/// </summary>
public class Blockchain
{
    public List<Block> Blocks { get; private set; }
    public int NextIndex { get; private set; } = 0;
    public static readonly int DIFFICULTY_FACTOR = 5;
    public static readonly int MAX_DIFFICULTY = 62;

    /// <summary>
    /// Creates a new blockchain
    /// </summary>
    public Blockchain()
    {
        var genesisTx = new Transaction(
            type: TransactionType.FEE,
            data: DateTime.UtcNow.ToString()
        );

        var genesisBlock = new Block(
            index: NextIndex,
            previousHash: "",
            transactions: new List<Transaction> { genesisTx }
        );

        Blocks = new List<Block> { genesisBlock };

        NextIndex++;
    }

    public Block GetLastBlock()
    {
        return Blocks.Last();
    }

    public int GetDifficulty()
    {
        return (int)Math.Ceiling((double)Blocks.Count / DIFFICULTY_FACTOR);
    }

    public Validation AddBlock(Block block)
    {
        var lastBlock = GetLastBlock();

        var validation = block.IsValid(lastBlock.Hash, lastBlock.Index, GetDifficulty());
        if (!validation.Success)
            return new Validation(false, $"Invalid block: {validation.Message}");

        Blocks.Add(block);
        NextIndex++;

        return new Validation();
    }

    public Block? GetBlock(string hash)
    {
        return Blocks.FirstOrDefault(b => b.Hash == hash);
    }

    public Validation IsValid()
    {
        for (int i = Blocks.Count - 1; i > 0; i--)
        {
            var currentBlock = Blocks[i];
            var previousBlock = Blocks[i - 1];
            var validation = currentBlock.IsValid(
                previousBlock.Hash, previousBlock.Index, GetDifficulty());
            if (!validation.Success)
                return new Validation(false,
                    $"Invalid block #{currentBlock.Index}: {validation.Message}");
        }

        return new Validation();
    }

    public int GetFeePerTx()
    {
        return 1;
    }

    public BlockInfo GetNextBlock()
    {
        var transactions = new List<Transaction>
        {
            new Transaction(
                type: TransactionType.REGULAR,
                data: DateTime.UtcNow.ToString()
            )
        };
        var difficulty = GetDifficulty();
        var previousHash = GetLastBlock().Hash;
        var index = Blocks.Count;
        var feePerTx = GetFeePerTx();
        var maxDifficulty = MAX_DIFFICULTY;

        return new BlockInfo
        {
            Transactions = transactions,
            Difficulty = difficulty,
            PreviousHash = previousHash,
            Index = index,
            FeePerTx = feePerTx,
            MaxDifficulty = maxDifficulty
        };
    }
}
