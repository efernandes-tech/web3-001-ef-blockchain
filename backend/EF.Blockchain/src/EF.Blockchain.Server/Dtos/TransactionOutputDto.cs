using EF.Blockchain.Domain;
using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Server.Dtos;

[ExcludeFromCodeCoverage]
public class TransactionOutputDto
{
    public string? ToAddress { get; set; } = string.Empty;
    public long? Amount { get; set; } = 0;
    public string? Tx { get; set; } = string.Empty;

    public TransactionOutput ToDomain()
    {
        return new TransactionOutput(ToAddress, (int)(Amount ?? 0), Tx);
    }
}
