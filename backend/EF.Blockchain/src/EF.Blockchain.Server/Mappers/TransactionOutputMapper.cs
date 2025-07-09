using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class TransactionOutputMapper
{
    public static TransactionOutput ToDomain(TransactionOutputDto dto)
    {
        return new TransactionOutput(
            toAddress: dto.ToAddress ?? string.Empty,
            amount: dto.Amount ?? 0,
            tx: dto.Tx
        );
    }

    public static TransactionOutputDto ToDto(TransactionOutput domain)
    {
        return new TransactionOutputDto
        {
            ToAddress = domain.ToAddress,
            Amount = domain.Amount,
            Tx = domain.Tx
        };
    }
}
