namespace EF.Blockchain.Server.Dtos;

public class BlockDto
{
    public int Index { get; set; }
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = null;
    public string PreviousHash { get; set; } = string.Empty;
    public List<TransactionDto> Transactions { get; set; } = new();
    public int? Nonce { get; set; } = null;
    public string? Miner { get; set; } = null;
}
