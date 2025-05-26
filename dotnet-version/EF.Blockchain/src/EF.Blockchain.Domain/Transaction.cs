using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

/// <summary>
/// Transaction class
/// </summary>
public class Transaction
{
    public TransactionType Type { get; private set; }
    public long Timestamp { get; private set; }
    public string Hash { get; private set; }
    public string Data { get; private set; }

    public Transaction(TransactionType? type = null, long? timestamp = null, string? data = null)
    {
        Type = type ?? TransactionType.REGULAR;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Data = data ?? string.Empty;
        Hash = GetHash();
    }

    /// <summary>
    /// Calculates the SHA256 hash of the transaction.
    /// </summary>
    public string GetHash()
    {
        var raw = $"{Type}{Data}{Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Validates the transaction.
    /// </summary>
    public Validation IsValid()
    {
        if (Hash != GetHash())
            return new Validation(false, "Invalid hash.");

        if (string.IsNullOrWhiteSpace(Data))
            return new Validation(false, "Invalid data.");

        return new Validation(); // success
    }
}
