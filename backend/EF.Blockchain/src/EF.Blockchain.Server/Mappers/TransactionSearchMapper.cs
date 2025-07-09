using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class TransactionSearchMapper
{
    public static TransactionSearchDto ToDto(TransactionSearch domain)
    {
        var transaction = TransactionMapper.ToDto(domain.Transaction);

        return new TransactionSearchDto
        {
            Transaction = transaction,
            MempoolIndex = domain.MempoolIndex,
            BlockIndex = domain.BlockIndex
        };
    }
}
