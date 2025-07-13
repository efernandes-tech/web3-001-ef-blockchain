using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;
using EF.Blockchain.Server.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EF.Blockchain.Server.Endpoints;

/// <summary>
/// Provides endpoints for managing blockchain transactions.
/// </summary>
public static class TransactionEndpoints
{
    /// <summary>
    /// Maps transaction-related endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/transactions/prepare", PrepareTransaction)
           .WithName("PrepareTransaction")
           .WithTags("Transaction")
           .WithSummary("Prepare a transaction with proper signing and hash generation")
           .WithDescription("Builds a complete transaction with signed inputs, proper outputs, and generates the transaction hash.")
           .Produces<TransactionDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
           .WithOpenApi();

        app.MapGet("/transactions/{hash?}", GetTransaction)
            .WithName("GetTransaction")
            .WithTags("Transaction")
            .WithSummary("Get a transaction by hash or list pending transactions")
            .WithDescription("If a hash is provided, returns the transaction. Otherwise, returns the next N pending transactions in the mempool.")
            .Produces<TransactionSearchDto?>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapPost("/transactions", PostTransaction)
            .WithName("PostTransaction")
            .WithTags("Transaction")
            .WithSummary("Submit a new transaction")
            .WithDescription("Receives a transaction DTO and attempts to add it to the blockchain mempool.")
            .Produces<TransactionDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();
    }

    /// <summary>
    /// Handles GET /transactions/{hash?}
    /// </summary>
    private static IResult GetTransaction(
        [FromRoute] string? hash,
        [FromServices] Domain.Blockchain blockchain)
    {
        if (!string.IsNullOrEmpty(hash))
        {
            TransactionSearch transactionSearch = blockchain.GetTransaction(hash);

            return transactionSearch is not null
                ? Results.Ok(TransactionSearchMapper.ToDto(transactionSearch))
                : Results.NotFound();
        }

        var next = blockchain.Mempool.Take(Domain.Blockchain.TX_PER_BLOCK).ToList();
        var total = blockchain.Mempool.Count;

        return Results.Ok(new { next, total });
    }

    /// <summary>
    /// Handles POST /transactions
    /// </summary>
    private static IResult PostTransaction(
        [FromBody] TransactionDto transactionDto,
        [FromServices] Domain.Blockchain blockchain)
    {
        if (string.IsNullOrEmpty(transactionDto.Hash))
            return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);

        var tx = TransactionMapper.ToDomain(transactionDto);
        var validation = blockchain.AddTransaction(tx);

        return validation.Success
            ? Results.Created($"/transactions/{tx.Hash}", TransactionMapper.ToDto(tx))
            : Results.BadRequest(validation);
    }

    /// <summary>
    /// Handles the POST /transactions/prepare endpoint.
    /// </summary>
    private static IResult PrepareTransaction(
        [FromBody] TransactionDto TransactionDto,
        [FromServices] Domain.Blockchain blockchain)
    {
        try
        {
            var fromWalletBalance = blockchain.GetBalance(TransactionDto.FromWalletAddress ?? "");
            var fee = blockchain.GetFeePerTx();
            var utxos = blockchain.GetUtxo(TransactionDto.FromWalletAddress ?? "");

            if (fromWalletBalance < TransactionDto.Amount + fee)
            {
                return Results.BadRequest(new Validation(false, "Insufficient balance"));
            }

            if (!utxos.Any())
            {
                return Results.BadRequest(new Validation(false, "No unspent transaction outputs available"));
            }

            var txInputs = utxos
                .Select(TransactionInput.FromTxo)
                .ToList();

            txInputs.ForEach(input => input.Sign(TransactionDto.FromWalletPrivateKey ?? ""));

            var txOutputs = new List<TransactionOutput>
            {
                new TransactionOutput(
                    toAddress: TransactionDto.ToWalletAddress ?? "",
                    amount: TransactionDto.Amount
                )
            };

            var remaining = fromWalletBalance - TransactionDto.Amount - fee;
            if (remaining > 0)
            {
                txOutputs.Add(new TransactionOutput(
                    toAddress: TransactionDto.FromWalletAddress ?? "",
                    amount: remaining
                ));
            }

            var transaction = new Transaction(
                type: TransactionType.REGULAR,
                timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                txInputs: txInputs,
                txOutputs: txOutputs
            );

            var transactionPrepare = TransactionMapper.ToDto(transaction);

            return Results.Ok(transactionPrepare);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Transaction preparation failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
