namespace EF.Blockchain.Domain;

/// <summary>
/// Represents the core blockchain logic, managing blocks, transactions, mining, and validation.
/// </summary>
public class Blockchain
{
    /// <summary>
    /// All confirmed blocks in the blockchain.
    /// </summary>
    public List<Block> Blocks { get; private set; }

    /// <summary>
    /// Pool of pending transactions waiting to be mined.
    /// </summary>
    public List<Transaction> Mempool { get; private set; }

    /// <summary>
    /// Index to be used for the next block.
    /// </summary>
    public int NextIndex { get; private set; } = 0;

    /// <summary>
    /// Number of blocks required to increase mining difficulty.
    /// </summary>
    public static readonly int DIFFICULTY_FACTOR = 5;

    /// <summary>
    /// Maximum number of transactions per block.
    /// </summary>
    public static readonly int TX_PER_BLOCK = 2;

    /// <summary>
    /// Maximum mining difficulty.
    /// </summary>
    public static readonly int MAX_DIFFICULTY = 62;

    /// <summary>
    /// Initializes a new blockchain with a genesis block mined by the given miner.
    /// </summary>
    /// <param name="miner">The public key (wallet address) of the genesis miner.</param>
    public Blockchain(string miner)
    {
        Blocks = new List<Block>();
        Mempool = new List<Transaction>();

        var genesisBlock = CreateGenesis(miner);
        Blocks.Add(genesisBlock);
        NextIndex++;
    }

    private Block CreateGenesis(string miner)
    {
        var amount = GetRewardAmount(GetDifficulty());

        var txOutput = new TransactionOutput(toAddress: miner, amount: amount);

        var feeTx = Transaction.FromReward(txOutput);

        var blockGenesis = new Block(
            index: NextIndex,
            previousHash: "",
            transactions: new List<Transaction> { feeTx }
        );

        blockGenesis.Mine(GetDifficulty(), miner);

        return blockGenesis;
    }

    /// <summary>
    /// Returns the last confirmed block.
    /// </summary>
    public Block GetLastBlock()
    {
        return Blocks.Last();
    }

    /// <summary>
    /// Calculates the current mining difficulty based on the number of blocks.
    /// </summary>
    public int GetDifficulty()
    {
        return (int)Math.Ceiling((double)Blocks.Count / DIFFICULTY_FACTOR) + 1;
    }

    /// <summary>
    /// Attempts to add a transaction to the mempool after validating it.
    /// </summary>
    public Validation AddTransaction(Transaction transaction)
    {
        if (transaction.TxInputs != null && transaction.TxInputs.Any())
        {
            var from = transaction.TxInputs[0].FromAddress;

            var pendingTx = Mempool
                .Where(tx => tx.TxInputs != null)
                .SelectMany(tx => tx.TxInputs!)
                .Where(txi => txi.FromAddress == from)
                .ToList();

            if (pendingTx.Any())
                return new Validation(false, "This wallet has a pending transaction");

            var utxo = GetUtxo(from);
            foreach (var txi in transaction.TxInputs)
            {
                var match = utxo.FirstOrDefault(txo =>
                    txo.Tx == txi.PreviousTx && txo.Amount >= txi.Amount);

                if (match == null)
                    return new Validation(false, "Invalid tx: the TXO is already spent or nonexistent");
            }
        }

        var validation = transaction.IsValid(GetDifficulty(), GetFeePerTx());
        if (!validation.Success)
            return new Validation(false, "Invalid tx: " + validation.Message);

        if (Blocks.Any(b => b.Transactions.Any(tx => tx.Hash == transaction.Hash)))
            return new Validation(false, "Duplicated tx in blockchain");

        Mempool.Add(transaction);

        return new Validation(true, transaction.Hash);
    }

    /// <summary>
    /// Attempts to add a new mined block to the blockchain after validation.
    /// </summary>
    public Validation AddBlock(Block block)
    {
        var nextBlockInfo = GetNextBlock();
        if (nextBlockInfo == null)
            return new Validation(false, "There is no next block info");

        var validation = block.IsValid(
            nextBlockInfo.PreviousHash,
            nextBlockInfo.Index - 1,
            nextBlockInfo.Difficulty,
            nextBlockInfo.FeePerTx
        );

        if (!validation.Success)
            return new Validation(false, $"Invalid block: {validation.Message}");

        var txs = block.Transactions
            .Where(tx => tx.Type != TransactionType.FEE)
            .Select(tx => tx.Hash)
            .ToList();

        var newMempool = Mempool
            .Where(tx => !txs.Contains(tx.Hash))
            .ToList();

        if (newMempool.Count + txs.Count != Mempool.Count)
            return new Validation(false, "Invalid tx in block: mempool");

        Mempool = newMempool;
        Blocks.Add(block);
        NextIndex++;

        return new Validation(true, block.Hash);
    }

    /// <summary>
    /// Finds a block by its hash.
    /// </summary>
    public Block? GetBlock(string hash)
    {
        return Blocks.FirstOrDefault(b => b.Hash == hash);
    }

    /// <summary>
    /// Finds a transaction by hash in the mempool or blockchain.
    /// </summary>
    public TransactionSearch GetTransaction(string hash)
    {
        var mempoolIndex = Mempool.FindIndex(tx => tx.Hash == hash);
        if (mempoolIndex != -1)
        {
            return new TransactionSearch(
                transaction: Mempool[mempoolIndex],
                mempoolIndex: mempoolIndex,
                blockIndex: -1
            );
        }

        var blockIndex = Blocks.FindIndex(b => b.Transactions.Any(tx => tx.Hash == hash));
        if (blockIndex != -1)
        {
            var transaction = Blocks[blockIndex].Transactions.First(tx => tx.Hash == hash);
            return new TransactionSearch(
                transaction: transaction,
                mempoolIndex: -1,
                blockIndex: blockIndex
            );
        }

        return new TransactionSearch(
            blockIndex: -1,
            mempoolIndex: -1,
            transaction: null!
        );
    }

    /// <summary>
    /// Validates the integrity of the entire blockchain from the latest block to the genesis.
    /// </summary>
    public Validation IsValid()
    {
        for (int i = Blocks.Count - 1; i > 0; i--)
        {
            var currentBlock = Blocks[i];
            var previousBlock = Blocks[i - 1];
            var validation = currentBlock.IsValid(
                previousBlock.Hash,
                previousBlock.Index,
                GetDifficulty(),
                GetFeePerTx()
            );

            if (!validation.Success)
                return new Validation(false, $"Invalid block #{currentBlock.Index}: {validation.Message}");
        }
        return new Validation();
    }

    /// <summary>
    /// Returns the current fixed fee per transaction.
    /// </summary>
    public int GetFeePerTx()
    {
        return 1;
    }

    /// <summary>
    /// Builds a <see cref="BlockInfo"/> object with data needed to mine the next block.
    /// </summary>
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

        return new BlockInfo(
            transactions: transactions,
            difficulty: difficulty,
            previousHash: previousHash,
            index: index,
            feePerTx: feePerTx,
            maxDifficulty: maxDifficulty
        );
    }

    /// <summary>
    /// Returns all transaction inputs for a specific wallet.
    /// </summary>
    public List<TransactionInput> GetTxInputs(string wallet)
    {
        return Blocks
            .SelectMany(b => b.Transactions)
            .Where(tx => tx.TxInputs != null && tx.TxInputs.Any())
            .SelectMany(tx => tx.TxInputs!)
            .Where(txi => txi.FromAddress == wallet)
            .ToList();
    }

    /// <summary>
    /// Returns all transaction outputs for a specific wallet.
    /// </summary>
    public List<TransactionOutput> GetTxOutputs(string wallet)
    {
        return Blocks
            .SelectMany(b => b.Transactions)
            .Where(tx => tx.TxOutputs != null && tx.TxOutputs.Any())
            .SelectMany(tx => tx.TxOutputs!)
            .Where(txo => txo.ToAddress == wallet)
            .ToList();
    }

    /// <summary>
    /// Returns the list of unspent transaction outputs (UTXOs) for a wallet.
    /// </summary>
    public List<TransactionOutput> GetUtxo(string wallet)
    {
        var txIns = GetTxInputs(wallet);
        var txOuts = GetTxOutputs(wallet);

        if (txIns == null || txIns.Count == 0)
            return txOuts;

        foreach (var txi in txIns)
        {
            var index = txOuts.FindIndex(txo => txo.Amount == txi.Amount);

            if (index != -1)
                txOuts.RemoveAt(index);
        }

        return txOuts;
    }

    /// <summary>
    /// Returns the total balance of a wallet based on UTXOs.
    /// </summary>
    public int GetBalance(string wallet)
    {
        var utxo = GetUtxo(wallet);
        return utxo?.Sum(txo => txo.Amount) ?? 0;
    }

    /// <summary>
    /// Calculates the mining reward based on the current difficulty.
    /// </summary>
    public static int GetRewardAmount(int difficulty)
    {
        return (64 - difficulty) * 10;
    }
}
