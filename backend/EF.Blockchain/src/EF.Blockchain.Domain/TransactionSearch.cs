using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Domain;

/// <summary>
/// Represents the result of searching for a transaction, indicating its location in the mempool or a block.
/// </summary>
[ExcludeFromCodeCoverage]
public class TransactionSearch
{
    /// <summary>
    /// The transaction found during the search.
    /// </summary>
    public Transaction Transaction { get; private set; } = null!;

    /// <summary>
    /// The index of the transaction in the mempool, or -1 if not found there.
    /// </summary>
    public int MempoolIndex { get; private set; }

    /// <summary>
    /// The index of the block containing the transaction, or -1 if not found in the blockchain.
    /// </summary>
    public int BlockIndex { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionSearch"/> class.
    /// </summary>
    /// <param name="transaction">The transaction that was found.</param>
    /// <param name="mempoolIndex">The index in the mempool, or -1 if not found.</param>
    /// <param name="blockIndex">The index of the block, or -1 if not found.</param>
    public TransactionSearch(Transaction transaction, int mempoolIndex, int blockIndex)
    {
        Transaction = transaction;
        MempoolIndex = mempoolIndex;
        BlockIndex = blockIndex;
    }
}
