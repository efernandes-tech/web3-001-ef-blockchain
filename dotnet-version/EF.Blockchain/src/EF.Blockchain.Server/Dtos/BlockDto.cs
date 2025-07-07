using EF.Blockchain.Domain;
using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Server.Dtos;

[ExcludeFromCodeCoverage]
public class BlockDto
{
    public int Index { get; set; }
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = null;
    public string PreviousHash { get; set; } = string.Empty;
    public List<TransactionDto> Transactions { get; set; } = new();
    public int? Nonce { get; set; } = null;
    public string? Miner { get; set; } = null;

    public Block ToDomain()
    {
        var transactions = Transactions
            .Select(tx => tx.ToDomain())
            .ToList();

        return new Block(Index, PreviousHash, transactions, Timestamp, Hash, Nonce, Miner);
    }
}
