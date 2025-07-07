using EF.Blockchain.Domain;
using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Server.Dtos;

[ExcludeFromCodeCoverage]
public class TransactionInputDto
{
    public string? FromAddress { get; set; } = string.Empty;
    public int? Amount { get; set; } = null;
    public string? Signature { get; set; } = string.Empty;
    public string? PreviousTx { get; set; } = string.Empty;

    public TransactionInput ToDomain()
    {
        return new TransactionInput(FromAddress, Amount, Signature, PreviousTx);
    }
}
