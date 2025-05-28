using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockMockFactory
{
    public static Block Create(
        int index = 0,
        string? previousHash = null,
        List<Transaction>? transactions = null,
        long? timestamp = null,
        string? hash = null,
        int? nonce = null,
        string? miner = null)
    {
        var prevHash = previousHash ?? "abc";
        var trans = transactions ?? new List<Transaction>
        {
            new Transaction(data: $"Block {index}")
        };
        var transHash = string.Join("", trans.Select(t => t.Hash));
        var ts = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var n = nonce ?? 1;
        var m = miner ?? "ef";
        var computedHash = Block.ComputeHash(index, ts, transHash, prevHash, n, m);

        return new Block(index, prevHash, trans, ts, computedHash, nonce: n, miner: m);
    }

    public static Block CreateInvalid() => Create(index: -1, hash: "wrong");
}
