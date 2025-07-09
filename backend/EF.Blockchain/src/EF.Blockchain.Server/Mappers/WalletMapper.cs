using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Mappers;

public static class WalletMapper
{
    public static WalletDto ToDto(int balance, int fee, List<TransactionOutput> utxo)
    {
        return new WalletDto
        {
            Balance = balance,
            Fee = fee,
            Utxo = utxo.Select(TransactionOutputMapper.ToDto).ToList()
        };
    }
}
