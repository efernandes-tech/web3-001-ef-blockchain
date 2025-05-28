using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class TransactionMockFactory
{
    public static Transaction Create(
        TransactionType? type = null,
        long? timestamp = null,
        string? data = null,
        string? hash = null)
    {
        var tx = new Transaction(
            type: type ?? TransactionType.REGULAR,
            timestamp: timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data: data ?? "mocked data"
        );

        if (hash != null)
        {
            // Use reflection to change private/internal state
            typeof(Transaction)
                .GetProperty(nameof(Transaction.Hash))!
                .SetValue(tx, hash);
        }

        return tx;
    }

    public static Transaction CreateInvalidData() =>
        Create(data: "");

    public static Transaction CreateInvalidHash() =>
        Create(data: "tx", hash: "wrong-hash");
}
