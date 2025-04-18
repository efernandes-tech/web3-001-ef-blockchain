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

    public Validation AddBlock(Block block)
    {
        var lastBlock = GetLastBlock();

        var validation = block.IsValid(lastBlock.Hash, lastBlock.Index);
        if (!validation.Success)
            return new Validation(false, $"Invalid block: {validation.Message}");

        Blocks.Add(block);
        NextIndex++;

        return new Validation();
    }

    public Validation IsValid()
    {
        for (int i = Blocks.Count - 1; i > 0; i--)
        {
            var currentBlock = Blocks[i];
            var previousBlock = Blocks[i - 1];
            var validation = currentBlock.IsValid(previousBlock.Hash, previousBlock.Index);
            if (!validation.Success)
                return new Validation(false, $"Invalid block #{currentBlock.Index}: {validation.Message}");
        }

        return new Validation();
    }
}
