namespace EF.Blockchain.Server.Endpoints;

public static class StatusEndpoints
{
    public static void MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/status", (Domain.Blockchain blockchain) => new
        {
            mempool = blockchain.Mempool.Count,
            blocks = blockchain.Blocks.Count,
            isValid = blockchain.IsValid(),
            lastBlock = blockchain.Blocks.LastOrDefault()
        });
    }
}
