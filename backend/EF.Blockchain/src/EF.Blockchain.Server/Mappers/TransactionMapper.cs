using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class TransactionMapper
{
    public static Transaction ToDomain(TransactionDto dto)
    {
        var txInputs = dto.TxInputs?.Select(TransactionInputMapper.ToDomain).ToList();
        var txOutputs = dto.TxOutputs.Select(TransactionOutputMapper.ToDomain).ToList();

        return new Transaction(
            type: dto.Type,
            timestamp: dto.Timestamp,
            txInputs: txInputs,
            txOutputs: txOutputs);
    }

    public static TransactionDto ToDto(Transaction domain)
    {
        var txInputs = domain.TxInputs?.Select(TransactionInputMapper.ToDto).ToList();
        var txOutputs = domain.TxOutputs.Select(TransactionOutputMapper.ToDto).ToList();

        return new TransactionDto
        {
            Type = domain.Type,
            Timestamp = domain.Timestamp,
            TxInputs = txInputs,
            TxOutputs = txOutputs
        };
    }
}
