using EF.Blockchain.Domain;

namespace EF.Blockchain.Server.Dtos;

public class TransactionDto
{
    public TransactionType? Type { get; set; } = TransactionType.REGULAR;
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = string.Empty;
    public List<TransactionInputDto>? TxInputs { get; set; } = null;
    public List<TransactionOutputDto> TxOutputs { get; set; } = new();
    public string? FromWalletAddress { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? FromWalletPrivateKey { get; set; } = string.Empty;
    public string? ToWalletAddress { get; set; } = string.Empty;
}
