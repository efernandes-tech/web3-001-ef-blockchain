using System.Threading.Tasks;
using EF.Blockchain.Domain;
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

        app.MapPost("/wallets", CreateWallet)
           .WithName("CreateWallet")
           .WithTags("Wallet")
           .WithSummary("Create a new wallet")
           .WithDescription("Generates a new wallet with public key, private key, and mnemonic phrase.")
           .Accepts<WalletDto>("application/json")
           .Produces<WalletDto>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .WithOpenApi();

        app.MapPost("/wallets/recover", RecoverWallet)
           .WithName("RecoverWallet")
           .WithTags("Wallet")
           .WithSummary("Recover an existing wallet")
           .WithDescription("Recovers a wallet using private key or mnemonic phrase.")
           .Accepts<WalletDto>("application/json")
           .Produces<WalletDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .ProducesValidationProblem()
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

    /// <summary>
    /// Handles the POST /wallets endpoint.
    /// </summary>
    private static async Task<IResult> CreateWallet([FromBody] WalletDto walletDto,
        IConfiguration config,
        [FromServices] Domain.Blockchain blockchain)
    {
        try
        {
            var newWallet = new Wallet();
            var amountFaucet = 10;

            var miner = config["Blockchain:MinerWallet:PrivateKey"]
                ?? Environment.GetEnvironmentVariable("BLOCKCHAIN_MINER")
                ?? "default-miner";

            var minerWallet = new Wallet(miner);

            var fromWalletBalance = blockchain.GetBalance(minerWallet.PublicKey ?? "");
            var fee = blockchain.GetFeePerTx();
            var utxos = blockchain.GetUtxo(minerWallet.PublicKey ?? "");

            var txInputs = utxos
                .Select(TransactionInput.FromTxo)
                .ToList();

            txInputs.ForEach(input => input.Sign(minerWallet.PrivateKey ?? ""));

            var txOutputs = new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: newWallet.PublicKey ?? "",
                    amount: amountFaucet
                )
            };

            var remaining = fromWalletBalance - amountFaucet - fee;
            if (remaining > 0)
            {
                txOutputs.Add(new TransactionOutput(
                    toAddress: minerWallet.PublicKey ?? "",
                    amount: remaining
                ));
            }

            var faucetTransaction = new Transaction(
                type: TransactionType.REGULAR,
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                txInputs: txInputs,
                txOutputs: txOutputs
            );

            blockchain.AddTransaction(faucetTransaction);

            await Task.Delay(2000);

            walletDto.PublicKey = newWallet.PublicKey;
            walletDto.PrivateKey = newWallet.PrivateKey;
            walletDto.Balance = amountFaucet;

            return Results.Created($"/wallets/{walletDto.PublicKey}", walletDto);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Wallet Creation Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Handles the POST /wallets/recover endpoint.
    /// </summary>
    /// <param name="walletDto">The wallet recovery request.</param>
    /// <param name="blockchain">Injected blockchain instance.</param>
    /// <returns>The recovered wallet information.</returns>
    private static IResult RecoverWallet(
        [FromBody] WalletDto walletDto,
        [FromServices] Domain.Blockchain blockchain)
    {
        try
        {
            Domain.Wallet recoveredWallet = new Domain.Wallet(walletDto.PrivateKey);

            int balance = blockchain.GetBalance(recoveredWallet.PublicKey);

            walletDto.PublicKey = recoveredWallet.PublicKey;
            walletDto.PrivateKey = recoveredWallet.PrivateKey;
            walletDto.Balance = balance;

            return Results.Ok(walletDto);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Wallet Recovery Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
