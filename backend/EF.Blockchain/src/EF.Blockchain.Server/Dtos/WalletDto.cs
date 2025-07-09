namespace EF.Blockchain.Server.Dtos;

public class WalletDto
{
    public int Balance { get; set; }
    public int Fee { get; set; }
    public List<TransactionOutputDto> Utxo { get; set; } = new();
}
