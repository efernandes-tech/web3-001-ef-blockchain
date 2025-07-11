using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class BlockMapper
{
    public static Block ToDomain(BlockDto dto)
    {
        if (dto == null)
            return null;

        var transactions = dto.Transactions?
            .Select(TransactionMapper.ToDomain)
            .ToList() ?? new List<Transaction>();

        return new Block(
            index: dto.Index,
            previousHash: dto.PreviousHash,
            transactions: transactions,
            timestamp: dto.Timestamp,
            hash: dto.Hash,
            nonce: dto.Nonce,
            miner: dto.Miner
        );
    }

    public static BlockDto ToDto(Block domain)
    {
        if (domain == null)
            return null;

        var transactions = domain.Transactions?
            .Select(TransactionMapper.ToDto)
            .ToList() ?? new List<TransactionDto>();

        return new BlockDto
        {
            Index = domain.Index,
            PreviousHash = domain.PreviousHash,
            Transactions = transactions,
            Timestamp = domain.Timestamp,
            Hash = domain.Hash,
            Nonce = domain.Nonce,
            Miner = domain.Miner
        };
    }
}
