using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Domain;

[ExcludeFromCodeCoverage]
public class TransactionSearch
{
    public Transaction Transaction { get; set; } = null!;
    public int MempoolIndex { get; set; }
    public int BlockIndex { get; set; }
}
