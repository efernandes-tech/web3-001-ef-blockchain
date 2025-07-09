using EF.Blockchain.Server.Dtos;
using EF.Blockchain.Server.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EF.Blockchain.Server.Endpoints;

/// <summary>
/// Provides endpoints to query wallet information.
/// </summary>
public static class WalletEndpoints
{
    /// <summary>
    /// Maps wallet-related endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/{walletAddress}", GetWalletInfo)
           .WithName("GetWalletInfo")
           .WithTags("Wallet")
           .WithSummary("Get wallet balance, fee, and UTXOs")
           .WithDescription("Returns balance, transaction fee, and unspent transaction outputs (UTXOs) for a wallet.")
           .Produces<WalletDto>(StatusCodes.Status200OK)
           .WithOpenApi();
    }

    /// <summary>
    /// Handles the GET /wallets/{walletAddress} endpoint.
    /// </summary>
    /// <param name="walletAddress">Wallet public address.</param>
    /// <param name="blockchain">Injected blockchain instance.</param>
    /// <returns>Wallet details including balance, fee, and UTXOs.</returns>
    private static WalletDto GetWalletInfo(
        [FromRoute] string walletAddress,
        [FromServices] Domain.Blockchain blockchain)
    {
        int balance = blockchain.GetBalance(walletAddress);
        int fee = blockchain.GetFeePerTx();

        var utxo = blockchain.GetUtxo(walletAddress);

        return WalletMapper.ToDto(balance, fee, utxo);
    }
}
