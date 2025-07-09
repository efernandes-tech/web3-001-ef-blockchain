namespace EF.Blockchain.Server.Dtos;

public class TransactionSearchDto
{
    public TransactionDto Transaction { get; set; } = null!;
    public int MempoolIndex { get; set; }
    public int BlockIndex { get; set; }
}
