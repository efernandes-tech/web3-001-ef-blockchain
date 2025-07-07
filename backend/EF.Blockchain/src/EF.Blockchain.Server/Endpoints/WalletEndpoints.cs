namespace EF.Blockchain.Server.Endpoints;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/{walletAddress}", (string walletAddress, Domain.Blockchain blockchain) =>
        {
            var utxo = blockchain.GetUtxo(walletAddress);
            int balance = blockchain.GetBalance(walletAddress);
            int fee = blockchain.GetFeePerTx();

            return Results.Json(new
            {
                balance,
                fee,
                utxo
            });
        });
    }
}
