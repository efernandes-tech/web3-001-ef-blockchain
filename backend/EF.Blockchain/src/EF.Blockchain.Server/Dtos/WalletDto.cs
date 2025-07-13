namespace EF.Blockchain.Server.Dtos;

public class WalletDto
{
    public int Balance { get; set; }
    public int Fee { get; set; }
    public List<TransactionOutputDto> Utxo { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}
