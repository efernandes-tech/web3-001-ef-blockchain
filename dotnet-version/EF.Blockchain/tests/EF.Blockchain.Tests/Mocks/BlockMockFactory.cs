using EF.Blockchain.Domain;

namespace EF.Blockchain.Tests.Mocks;

public static class BlockMockFactory
{
    public static Block Create(
        int index = 0,
        string? previousHash = null,
        string? data = null,
        long? timestamp = null,
        string? hash = null,
        int? nonce = null,
        string? miner = null)
    {
        var prevHash = previousHash ?? "abc";
        var content = data ?? $"Block {index}";
        var ts = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var n = nonce ?? 1;
        var m = miner ?? "ef";
        var computedHash = Block.ComputeHash(index, ts, content, prevHash, n, m);

        return new Block(index, prevHash, content, ts, computedHash, nonce: n, miner: m);
    }

    public static Block CreateInvalid() => Create(index: -1, hash: "wrong");
}
