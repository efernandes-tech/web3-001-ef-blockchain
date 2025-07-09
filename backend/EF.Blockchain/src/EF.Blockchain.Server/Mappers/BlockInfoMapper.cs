using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class BlockInfoMapper
{
    public static BlockInfoDto ToDto(BlockInfo domain)
    {
        var transactions = domain.Transactions
            .Select(TransactionMapper.ToDto)
            .ToList();

        return new BlockInfoDto
        {
            Index = domain.Index,
            PreviousHash = domain.PreviousHash,
            Difficulty = domain.Difficulty,
            MaxDifficulty = domain.MaxDifficulty,
            FeePerTx = domain.FeePerTx,
            Transactions = transactions
        };
    }
}
