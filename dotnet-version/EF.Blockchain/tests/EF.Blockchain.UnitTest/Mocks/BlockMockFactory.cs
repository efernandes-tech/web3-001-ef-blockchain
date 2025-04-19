using EF.Blockchain.Domain;

namespace EF.Blockchain.UnitTest.Mocks;

public static class BlockMockFactory
{
    public static Block Create(
        int index = 0,
        string? previousHash = null,
        string? data = null,
        long? timestamp = null,
        string? hash = null)
    {
        var prevHash = previousHash ?? string.Empty;
        var content = data ?? "mock-data";
        var ts = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new Block(index, prevHash, content, ts, hash);
    }

    public static Block CreateInvalid() => Create(index: -1, hash: "wrong");
}
