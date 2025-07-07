using System.Security.Cryptography;
using System.Text;

namespace EF.Blockchain.Domain;

public class TransactionOutput
{
    public string ToAddress { get; private set; }
    public int Amount { get; private set; }
    public string? Tx { get; private set; }

    public TransactionOutput(
        string toAddress = "",
        int amount = 0,
        string? tx = "")
    {
        ToAddress = toAddress.Trim();
        Amount = amount;
        Tx = tx;
    }

    public Validation IsValid()
    {
        if (string.IsNullOrWhiteSpace(ToAddress))
            return new Validation(false, "Missing address");

        if (Amount < 1)
            return new Validation(false, "Negative amount");

        return new Validation(true);
    }

    public string GetHash()
    {
        var input = $"{ToAddress}{Amount}";
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public void SetTx(string txHash)
    {
        Tx = txHash;
    }
}
