using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class TransactionInputMapper
{
    public static TransactionInput ToDomain(TransactionInputDto dto)
    {
        if (dto == null)
            return null;

        return new TransactionInput(
            fromAddress: dto.FromAddress ?? string.Empty,
            amount: dto.Amount ?? 0,
            signature: dto.Signature ?? string.Empty,
            previousTx: dto.PreviousTx ?? string.Empty
        );
    }

    public static TransactionInputDto ToDto(TransactionInput domain)
    {
        if (domain == null)
            return null;

        return new TransactionInputDto
        {
            FromAddress = domain.FromAddress,
            Amount = domain.Amount,
            Signature = domain.Signature,
            PreviousTx = domain.PreviousTx
        };
    }
}
