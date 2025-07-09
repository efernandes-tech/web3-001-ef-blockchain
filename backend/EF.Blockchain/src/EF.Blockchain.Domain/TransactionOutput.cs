using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

/// <summary>
/// Represents a transaction output in the blockchain.
/// Contains information about the receiver, the amount, and the parent transaction hash.
/// </summary>
public class TransactionOutput
{
    /// <summary>
    /// Gets the address of the receiver.
    /// </summary>
    public string ToAddress { get; private set; }

    /// <summary>
    /// Gets the amount being transferred.
    /// </summary>
    public int Amount { get; private set; }

    /// <summary>
    /// Gets or sets the transaction hash this output belongs to.
    /// </summary>
    public string? Tx { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionOutput"/> class.
    /// </summary>
    /// <param name="toAddress">The destination address.</param>
    /// <param name="amount">The amount to be transferred.</param>
    /// <param name="tx">Optional parent transaction hash.</param>
    public TransactionOutput(
        string toAddress = "",
        int amount = 0,
        string? tx = "")
    {
        ToAddress = toAddress.Trim();
        Amount = amount;
        Tx = tx;
    }

    /// <summary>
    /// Validates the transaction output fields.
    /// </summary>
    /// <returns>A <see cref="Validation"/> result indicating if the output is valid.</returns>
    public Validation IsValid()
    {
        if (string.IsNullOrWhiteSpace(ToAddress))
            return new Validation(false, "Missing address");

        if (Amount < 1)
            return new Validation(false, "Negative amount");

        return new Validation(true);
    }

    /// <summary>
    /// Generates a SHA256 hash based on the output's address and amount.
    /// </summary>
    /// <returns>A lowercase hexadecimal string representing the hash.</returns>
    public string GetHash()
    {
        var input = $"{ToAddress}{Amount}";
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Sets the parent transaction hash for this output.
    /// </summary>
    /// <param name="txHash">The transaction hash to associate.</param>
    public void SetTx(string txHash)
    {
        Tx = txHash;
    }
}
