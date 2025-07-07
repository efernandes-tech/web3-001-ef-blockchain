using EF.Blockchain.Domain;
using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Server.Dtos;

[ExcludeFromCodeCoverage]
public class TransactionDto
{
    public TransactionType? Type { get; set; } = TransactionType.REGULAR;
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = string.Empty;
    public List<TransactionInputDto>? TxInputs { get; set; } = null;
    public List<TransactionOutputDto> TxOutputs { get; set; } = new();

    public Transaction ToDomain()
    {
        var txInputs = TxInputs?.Select(i => i.ToDomain()).ToList();
        var txOutputs = TxOutputs.Select(o => o.ToDomain()).ToList();

        return new Transaction(
            type: Type,
            timestamp: Timestamp,
            txInputs: txInputs,
            txOutputs: txOutputs);
    }
}
