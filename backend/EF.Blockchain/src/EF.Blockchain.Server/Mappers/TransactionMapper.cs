using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class TransactionMapper
{
    public static Transaction ToDomain(TransactionDto dto)
    {
        if (dto == null)
            return null;

        var txInputs = dto.TxInputs?
            .Select(TransactionInputMapper.ToDomain)
            .ToList() ?? new List<TransactionInput>();

        var txOutputs = dto.TxOutputs?
            .Select(TransactionOutputMapper.ToDomain)
            .ToList() ?? new List<TransactionOutput>();

        return new Transaction(
            type: dto.Type,
            timestamp: dto.Timestamp,
            txInputs: txInputs,
            txOutputs: txOutputs);
    }

    public static TransactionDto ToDto(Transaction domain)
    {
        if (domain == null)
            return null;

        var txInputs = domain.TxInputs?
            .Select(TransactionInputMapper.ToDto)
            .ToList() ?? new List<TransactionInputDto>();

        var txOutputs = domain.TxOutputs?
            .Select(TransactionOutputMapper.ToDto)
            .ToList() ?? new List<TransactionOutputDto>();

        return new TransactionDto
        {
            Type = domain.Type,
            Timestamp = domain.Timestamp,
            Hash = domain.Hash,
            TxInputs = txInputs,
            TxOutputs = txOutputs
        };
    }
}
