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
    public List<TransactionInput>? TxInputs { get; private set; }
    public List<TransactionOutput> TxOutputs { get; private set; }

    [JsonConstructor]
    public Transaction(
        TransactionType type,
        long timestamp,
        string hash,
        List<TransactionInput>? txInputs = null,
        List<TransactionOutput>? txOutputs = null)
    {
        Type = type;
        Timestamp = timestamp;
        Hash = hash;
        TxInputs = txInputs;
        TxOutputs = txOutputs;
    }

    public Transaction(
        TransactionType? type = null,
        long? timestamp = null,
        List<TransactionInput>? txInputs = null,
        List<TransactionOutput>? txOutputs = null)
    {
        Type = type ?? TransactionType.REGULAR;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        TxInputs = txInputs;
        TxOutputs = txOutputs ?? new List<TransactionOutput>();

        Hash = GetHash();

        // Set tx reference hash for outputs
        foreach (var txo in TxOutputs)
        {
            txo.SetTx(Hash);
        }
    }

    /// <summary>
    /// Calculates the SHA256 hash of the transaction.
    /// </summary>
    public string GetHash()
    {
        var from = TxInputs != null && TxInputs.Any()
            ? string.Join(",", TxInputs.Select(txi => txi.Signature))
            : "";

        var to = TxOutputs != null && TxOutputs.Any()
            ? string.Join(",", TxOutputs.Select(txo => txo.GetHash()))
            : "";

        var raw = $"{Type}{from}{to}{Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(raw);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public int GetFee()
    {
        if (TxInputs == null || !TxInputs.Any())
            return 0;

        var inputSum = TxInputs.Sum(txi => txi.Amount);
        var outputSum = TxOutputs?.Sum(txo => txo.Amount) ?? 0;

        return inputSum - outputSum;
    }

    /// <summary>
    /// Validates the transaction.
    /// </summary>
    public Validation IsValid(int difficulty, int totalFees)
    {
        if (Hash != GetHash())
            return new Validation(false, "Invalid hash");

        if (TxOutputs == null || !TxOutputs.Any() || TxOutputs.Any(txo => !txo.IsValid().Success))
            return new Validation(false, "Invalid TXO");

        if (TxInputs != null && TxInputs.Any())
        {
            var inputValidation = TxInputs
                .Select(txi => txi.IsValid())
                .Where(v => !v.Success)
                .ToList();

            if (inputValidation.Any())
            {
                var message = string.Join(" ", inputValidation.Select(v => v.Message));
                return new Validation(false, $"Invalid tx: {message}");
            }

            var inputSum = TxInputs.Sum(txi => txi.Amount);
            var outputSum = TxOutputs.Sum(txo => txo.Amount);
            if (inputSum < outputSum)
                return new Validation(false, "Invalid tx: input amounts must be equals or greater than outputs amounts");
        }

        if (TxOutputs.Any(txo => txo.Tx != Hash))
            return new Validation(false, "Invalid TXO reference hash");

        if (Type == TransactionType.FEE)
        {
            var txo = TxOutputs[0];
            if (txo.Amount > Blockchain.GetRewardAmount(difficulty) + totalFees)
                return new Validation(false, "Invalid tx reward");
        }

        return new Validation();
    }

    public static Transaction FromReward(TransactionOutput txo)
    {
        var tx = new Transaction(
            type: TransactionType.FEE,
            txOutputs: new List<TransactionOutput> { txo }
        );

        tx.Hash = tx.GetHash();
        tx.TxOutputs[0].SetTx(tx.Hash);
        return tx;
    }
}
