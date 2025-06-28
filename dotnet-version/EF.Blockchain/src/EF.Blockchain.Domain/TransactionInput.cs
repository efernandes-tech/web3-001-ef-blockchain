using NBitcoin;
using NBitcoin.Crypto;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace EF.Blockchain.Domain;

public class TransactionInput
{
    public string FromAddress { get; private set; }
    public decimal Amount { get; private set; }
    public string Signature { get; private set; }
    public string PreviousTx { get; private set; }

    [JsonConstructor]
    public TransactionInput(string fromAddress, decimal amount, string signature, string previousTx)
    {
        FromAddress = fromAddress;
        Amount = amount;
        Signature = signature;
        PreviousTx = previousTx;
    }

    public TransactionInput(string? fromAddress = null, decimal? amount = null, string? signature = null, string? previousTx = null)
    {
        FromAddress = fromAddress ?? string.Empty;
        Amount = amount ?? 0;
        Signature = signature ?? string.Empty;
        PreviousTx = previousTx ?? string.Empty;
    }

    public string GetHash()
    {
        var raw = $"{PreviousTx}{FromAddress}{Amount}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    public void Sign(string privateKeyHex)
    {
        var privateKeyBytes = Convert.FromHexString(privateKeyHex);
        var key = new Key(privateKeyBytes);

        var hashHex = GetHash();
        var hashBytes = Convert.FromHexString(hashHex);
        var signature = key.Sign(new uint256(hashBytes));

        Signature = Convert.ToHexString(signature.ToDER());
    }

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
}
