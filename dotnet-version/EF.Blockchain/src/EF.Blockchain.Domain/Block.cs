using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

/// <summary>
/// Block class
/// </summary>
public class Block
{
    public int Index { get; private set; }
    public long Timestamp { get; private set; }
    public string Hash { get; private set; }
    public string PreviousHash { get; private set; }
    public string Data { get; private set; }

    /// <summary>
    /// Creates a new Block
    /// </summary>
    /// <param name="index">The block index in blockchain</param>
    /// <param name="previousHash">The previous block hash</param>
    /// <param name="data">The block data</param>
    public Block(int index, string previousHash, string data)
    {
        Index = index;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PreviousHash = previousHash;
        Data = data;
        Hash = GetHash();
    }

    /// <summary>
    /// Generates the hash for the block based on its data.
    /// </summary>
    /// <returns>The SHA-256 hash string.</returns>
    public string GetHash()
    {
        var rawData = $"{Index}{Data}{Timestamp}{PreviousHash}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawData);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes); // Uppercase hex string
    }

    /// <summary>
    /// Validates the block
    /// </summary>
    /// /// <param name="previousHash">The previous block hash</param>
    /// /// <param name="previousIndex">The previous block index</param>
    /// <returns><c>Validation</c> if the block is valid</returns>
    public Validation IsValid(string previousHash, int previousIndex)
    {
        if (previousIndex != Index - 1)
        {
            return new Validation(false, "Invalid index");
        }
        if (Hash != GetHash())
        {
            return new Validation(false, "Invalid hash");
        }
        if (string.IsNullOrEmpty(Data))
        {
            return new Validation(false, "Invalid data");
        }
        if (Timestamp <= 0)
        {
            return new Validation(false, "Invalid timestamp");
        }
        if (previousHash != PreviousHash)
        {
            return new Validation(false, "Invalid previous hash");
        }
        return new Validation();
    }

    public void SetTimestamp(long timestamp)
    {
        Timestamp = timestamp;
    }

    public void SetHash(string hash)
    {
        Hash = hash;
    }

    public void SetData(string data)
    {
        Data = data;
    }
}
