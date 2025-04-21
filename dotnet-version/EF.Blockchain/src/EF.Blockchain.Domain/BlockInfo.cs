using System.Diagnostics.CodeAnalysis;

namespace EF.Blockchain.Domain;

/// <summary>
/// The BlockInfo class
/// </summary>
[ExcludeFromCodeCoverage]
public class BlockInfo
{
    public int Index { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public int MaxDifficulty { get; set; }
    public int FeePerTx { get; set; }
    public string Data { get; set; } = string.Empty;
}
