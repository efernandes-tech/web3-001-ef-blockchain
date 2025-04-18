namespace EF.Blockchain.Domain;

/// <summary>
/// Blockchain class
/// </summary>
public class Blockchain
{
    public List<Block> Blocks { get; private set; }
    public int NextIndex { get; private set; } = 0;

    /// <summary>
    /// Creates a new blockchain
    /// </summary>
    public Blockchain()
    {
        Blocks = new List<Block> { new Block(NextIndex, "", "Genesis Block") };
        NextIndex++;
    }

    public Block GetLastBlock()
    {
        return Blocks.Last();
    }

    public bool AddBlock(Block block)
    {
        var lastBlock = GetLastBlock();

        if (!block.IsValid(lastBlock.Hash, lastBlock.Index))
            return false;

        Blocks.Add(block);
        NextIndex++;

        return true;
    }

    public bool IsValid()
    {
        for (int i = Blocks.Count - 1; i > 0; i--)
        {
            var currentBlock = Blocks[i];
            var previousBlock = Blocks[i - 1];
            var isValid = currentBlock.IsValid(previousBlock.Hash, previousBlock.Index);
            if (!isValid)
                return false;
        }

        return true;
    }
}
