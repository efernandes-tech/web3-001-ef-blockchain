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
    public static readonly int TX_PER_BLOCK = 2;
    public static readonly int MAX_DIFFICULTY = 62;

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

    public Block GetLastBlock()
    {
        return Blocks.Last();
    }

    public int GetDifficulty()
    {
        return (int)Math.Ceiling((double)Blocks.Count / DIFFICULTY_FACTOR) + 1;
    }

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
                Transaction = Mempool[mempoolIndex],
                BlockIndex = -1,
            };
        }

        var blockIndex = Blocks.FindIndex(b => b.Transactions.Any(tx => tx.Hash == hash));
        if (blockIndex != -1)
        {
            var transaction = Blocks[blockIndex].Transactions.First(tx => tx.Hash == hash);
            return new TransactionSearch
            {
                BlockIndex = blockIndex,
                Transaction = transaction,
                MempoolIndex = -1
            };
        }

        return new TransactionSearch
        {
            BlockIndex = -1,
            MempoolIndex = -1,
            Transaction = null!
        };
    }

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

    public List<TransactionInput> GetTxInputs(string wallet)
    {
        return Blocks
            .SelectMany(b => b.Transactions)
            .Where(tx => tx.TxInputs != null && tx.TxInputs.Any())
            .SelectMany(tx => tx.TxInputs!)
            .Where(txi => txi.FromAddress == wallet)
            .ToList();
    }

    public List<TransactionOutput> GetTxOutputs(string wallet)
    {
        return Blocks
            .SelectMany(b => b.Transactions)
            .Where(tx => tx.TxOutputs != null && tx.TxOutputs.Any())
            .SelectMany(tx => tx.TxOutputs!)
            .Where(txo => txo.ToAddress == wallet)
            .ToList();
    }

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

    public int GetBalance(string wallet)
    {
        var utxo = GetUtxo(wallet);
        return utxo?.Sum(txo => txo.Amount) ?? 0;
    }

    public static int GetRewardAmount(int difficulty)
    {
        return (64 - difficulty) * 10;
    }
}
