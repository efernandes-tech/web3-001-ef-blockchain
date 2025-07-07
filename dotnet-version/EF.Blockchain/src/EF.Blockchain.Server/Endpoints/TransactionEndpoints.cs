using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/{hash?}", (string? hash, Domain.Blockchain blockchain) =>
        {
            if (!string.IsNullOrEmpty(hash))
            {
                var transaction = blockchain.GetTransaction(hash);
                return Results.Json(transaction);
            }

            var next = blockchain.Mempool.Take(Domain.Blockchain.TX_PER_BLOCK).ToList();
            var total = blockchain.Mempool.Count;

            return Results.Json(new { next, total });
        });

        app.MapPost("/transactions", (TransactionDto transactionDto, Domain.Blockchain blockchain) =>
        {
            if (string.IsNullOrEmpty(transactionDto.Hash))
                return Results.StatusCode(422);

            var tx = transactionDto.ToDomain();

            var validation = blockchain.AddTransaction(tx);

            return validation.Success
                ? Results.Created("/transactions", tx)
                : Results.BadRequest(validation);
        });
    }
}
