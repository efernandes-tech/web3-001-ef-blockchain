namespace EF.Blockchain.Server.Dtos;

public class BlockInfoDto
{
    public int Index { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int MaxDifficulty { get; set; }
    public int FeePerTx { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
}
