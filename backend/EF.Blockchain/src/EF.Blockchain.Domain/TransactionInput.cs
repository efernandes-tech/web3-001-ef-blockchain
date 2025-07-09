using NBitcoin;
using NBitcoin.Crypto;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace EF.Blockchain.Domain;

/// <summary>
/// Represents an input in a blockchain transaction, including signature validation and ownership of funds.
/// </summary>
public class TransactionInput
{
    /// <summary>
    /// Public key (hex) of the sender.
    /// </summary>
    public string FromAddress { get; private set; }

    /// <summary>
    /// Amount of coins being transferred from this input.
    /// </summary>
    public int Amount { get; private set; }

    /// <summary>
    /// Signature proving ownership of the referenced output.
    /// </summary>
    public string Signature { get; private set; }

    /// <summary>
    /// The hash of the previous transaction output being referenced.
    /// </summary>
    public string PreviousTx { get; private set; }

    /// <summary>
    /// Constructor used for deserialization.
    /// </summary>
    [JsonConstructor]
    public TransactionInput(string fromAddress, int amount, string signature, string previousTx)
    {
        FromAddress = fromAddress;
        Amount = amount;
        Signature = signature;
        PreviousTx = previousTx;
    }

    /// <summary>
    /// Creates a new transaction input with optional values.
    /// </summary>
    public TransactionInput(string? fromAddress = null, int? amount = null, string? signature = null, string? previousTx = null)
    {
        FromAddress = fromAddress ?? string.Empty;
        Amount = amount ?? 0;
        Signature = signature ?? string.Empty;
        PreviousTx = previousTx ?? string.Empty;
    }

    /// <summary>
    /// Computes a SHA-256 hash of this transaction input.
    /// </summary>
    /// <returns>The lowercase hex hash string.</returns>
    public string GetHash()
    {
        var raw = $"{PreviousTx}{FromAddress}{Amount}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Signs this transaction input using the sender's private key.
    /// </summary>
    /// <param name="privateKeyHex">The sender's private key in hex format.</param>
    public void Sign(string privateKeyHex)
    {
        var privateKeyBytes = Convert.FromHexString(privateKeyHex);
        var key = new Key(privateKeyBytes);

        var hashHex = GetHash();
        var hashBytes = Convert.FromHexString(hashHex);
        var signature = key.Sign(new uint256(hashBytes));

        Signature = Convert.ToHexString(signature.ToDER());
    }

    /// <summary>
    /// Validates the transaction input by checking the digital signature.
    /// </summary>
    /// <returns>A <see cref="Validation"/> result indicating success or failure.</returns>
    public Validation IsValid()
    {
        if (string.IsNullOrWhiteSpace(Signature) || string.IsNullOrWhiteSpace(PreviousTx))
            return new Validation(false, "Signature and previous TX are required");

        if (Amount < 1)
            return new Validation(false, "Amount must be greater than zero");

        try
        {
            var pubKeyBytes = Convert.FromHexString(FromAddress);
            var pubKey = new PubKey(pubKeyBytes);

            var hashBytes = Convert.FromHexString(GetHash());
            var signatureBytes = Convert.FromHexString(Signature);
            var ecdsaSig = ECDSASignature.FromDER(signatureBytes);

            bool isValid = pubKey.Verify(new uint256(hashBytes), ecdsaSig);

            return isValid ? new Validation() : new Validation(false, "Invalid tx input signature");
        }
        catch
        {
            return new Validation(false, "Error verifying signature");
        }
    }

    /// <summary>
    /// Creates a new <see cref="TransactionInput"/> from a given <see cref="TransactionOutput"/>.
    /// </summary>
    /// <param name="txo">The transaction output to reference.</param>
    /// <returns>A new input referencing the output.</returns>
    public static TransactionInput FromTxo(TransactionOutput txo)
    {
        return new TransactionInput(
            fromAddress: txo.ToAddress,
            amount: txo.Amount,
            previousTx: txo.Tx
        );
    }
}
