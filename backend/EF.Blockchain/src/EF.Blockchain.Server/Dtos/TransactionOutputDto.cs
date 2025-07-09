namespace EF.Blockchain.Server.Dtos;

public class TransactionOutputDto
{
    public string? ToAddress { get; set; } = string.Empty;
    public int? Amount { get; set; } = 0;
    public string? Tx { get; set; } = string.Empty;
}
