namespace EF.Blockchain.Server.Dtos;

public class StatusDto
{
    public int Mempool { get; set; }
    public int Blocks { get; set; }
    public bool IsValid { get; set; }
    public BlockDto? LastBlock { get; set; }
}
