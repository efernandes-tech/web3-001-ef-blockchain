namespace EF.Blockchain.Server.Dtos;

public class TransactionInputDto
{
    public string? FromAddress { get; set; } = string.Empty;
    public int? Amount { get; set; } = null;
    public string? Signature { get; set; } = string.Empty;
    public string? PreviousTx { get; set; } = string.Empty;
}
