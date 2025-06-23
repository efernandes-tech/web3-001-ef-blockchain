using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace EF.Blockchain.Domain;

/// <summary>
/// Transaction class
/// </summary>
public class Transaction
{
    public TransactionType Type { get; private set; }
    public long Timestamp { get; private set; }
    public string Hash { get; private set; }
    public TransactionInput? TxInput { get; private set; }
    public string To { get; private set; }

    [JsonConstructor]
    public Transaction(TransactionType type, long timestamp, string hash, TransactionInput txInput, string to)
    {
        Type = type;
        Timestamp = timestamp;
        Hash = hash;
        TxInput = txInput;
        To = to;
    }

    public Transaction(TransactionType? type = null, long? timestamp = null, TransactionInput? txInput = null, string? to = null)
    {
        Type = type ?? TransactionType.REGULAR;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        TxInput = txInput ?? null;
        To = to ?? string.Empty;
        Hash = GetHash();
    }

    /// <summary>
    /// Calculates the SHA256 hash of the transaction.
    /// </summary>
    public string GetHash()
    {
        var fromHash = TxInput != null ? TxInput.GetHash() : "";
        var raw = $"{Type}{fromHash}{To}{Timestamp}";
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

        if (string.IsNullOrWhiteSpace(To))
            return new Validation(false, "Invalid 'to' address.");

        if (TxInput != null)
        {
            var inputValidation = TxInput.IsValid();
            if (!inputValidation.Success)
                return new Validation(false, $"Invalid TxInput: {inputValidation.Message}");
        }

        return new Validation();
    }
}
