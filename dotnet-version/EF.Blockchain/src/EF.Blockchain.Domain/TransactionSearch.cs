namespace EF.Blockchain.Domain;

public class TransactionSearch
{
    public Transaction Transaction { get; set; } = null!;
    public int MempoolIndex { get; set; }
    public int BlockIndex { get; set; }
}
