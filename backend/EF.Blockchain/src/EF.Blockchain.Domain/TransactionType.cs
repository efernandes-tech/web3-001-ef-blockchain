namespace EF.Blockchain.Domain;

/// <summary>
/// Defines the type of a blockchain transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// A standard transaction transferring value between addresses.
    /// </summary>
    REGULAR = 1,

    /// <summary>
    /// A special transaction that rewards the miner (e.g., block reward + fees).
    /// </summary>
    FEE = 2
}
