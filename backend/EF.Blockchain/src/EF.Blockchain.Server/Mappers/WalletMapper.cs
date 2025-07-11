using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class WalletMapper
{
    public static WalletDto ToDto(int balance, int fee, List<TransactionOutput> utxo)
    {
        var utxoList = utxo?
            .Select(TransactionOutputMapper.ToDto)
            .ToList() ?? new List<TransactionOutputDto>();

        return new WalletDto
        {
            Balance = balance,
            Fee = fee,
            Utxo = utxoList
        };
    }
}
