namespace EF.Blockchain.Domain;

/// <summary>
/// Blockchain class
/// </summary>
public class Blockchain
{
    public List<Block> Blocks { get; private set; }
    public List<Transaction> Mempool { get; private set; }
    public int NextIndex { get; private set; } = 0;
    public static readonly int DIFFICULTY_FACTOR = 5;
    public static readonly int TX_PER_BLOCK = 2; // Max transactions per block
    public static readonly int MAX_DIFFICULTY = 62;

    /// <summary>
    /// Creates a new blockchain
    /// </summary>
    public Blockchain()
    {
        var genesisTx = new Transaction(
            type: TransactionType.FEE,
            txInput: new TransactionInput()
        );

        var genesisBlock = new Block(
            index: NextIndex,
            previousHash: "",
            transactions: new List<Transaction> { genesisTx }
        );

        Blocks = new List<Block> { genesisBlock };

        Mempool = new List<Transaction>();

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

    public Validation AddTransaction(Transaction transaction)
    {
        if (transaction.TxInput != null)
        {
            var from = transaction.TxInput.FromAddress;

            var pendingTx = Mempool
                .Where(tx => tx.TxInput != null && tx.TxInput.FromAddress == from)
                .ToList();

            if (pendingTx.Count > 0)
                return new Validation(false, $"This wallet has a pending transaction.");

            // TODO: Validate funds origin here if needed
        }

        var validation = transaction.IsValid();
        if (!validation.Success)
            return new Validation(false, "Invalid tx: " + validation.Message);

        if (Blocks.Any(b => b.Transactions.Any(tx => tx.Hash == transaction.Hash)))
            return new Validation(false, "Duplicated tx in blockchain.");

        if (Mempool.Any(tx => tx.Hash == transaction.Hash))
            return new Validation(false, "Duplicated tx in mempool.");

        Mempool.Add(transaction);
        return new Validation(true, transaction.Hash);
    }

    public Validation AddBlock(Block block)
    {
        var lastBlock = GetLastBlock();

        var validation = block.IsValid(lastBlock.Hash, lastBlock.Index, GetDifficulty());
        if (!validation.Success)
            return new Validation(false, $"Invalid block: {validation.Message}");

        // Filter out non-fee transactions from the block
        var txHashes = block.Transactions
            .Where(tx => tx.Type != TransactionType.FEE)
            .Select(tx => tx.Hash)
            .ToList();

        // Remove block transactions from the mempool
        var newMempool = Mempool
            .Where(tx => !txHashes.Contains(tx.Hash))
            .ToList();

        // Validate that all block txs came from the mempool
        if (newMempool.Count + txHashes.Count != Mempool.Count)
            return new Validation(false, "Invalid tx in block: mempool");

        // Update the mempool
        Mempool = newMempool;

        Blocks.Add(block);
        NextIndex++;

        return new Validation(true, block.Hash);
    }

    public Block? GetBlock(string hash)
    {
        return Blocks.FirstOrDefault(b => b.Hash == hash);
    }

    public TransactionSearch GetTransaction(string hash)
    {
        var mempoolIndex = Mempool.FindIndex(tx => tx.Hash == hash);
        if (mempoolIndex != -1)
        {
            return new TransactionSearch
            {
                MempoolIndex = mempoolIndex,
                Transaction = Mempool[mempoolIndex]
            };
        }

        var blockIndex = Blocks.FindIndex(b => b.Transactions.Any(tx => tx.Hash == hash));
        if (blockIndex != -1)
        {
            var transaction = Blocks[blockIndex].Transactions.First(tx => tx.Hash == hash);
            return new TransactionSearch
            {
                BlockIndex = blockIndex,
                Transaction = transaction
            };
        }

        return new TransactionSearch
        {
            BlockIndex = -1,
            MempoolIndex = -1,
            Transaction = null! // or handle null properly
        };
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

    public BlockInfo? GetNextBlock()
    {
        if (Mempool == null || Mempool.Count == 0)
            return null;

        var transactions = Mempool.Take(Blockchain.TX_PER_BLOCK).ToList();

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
