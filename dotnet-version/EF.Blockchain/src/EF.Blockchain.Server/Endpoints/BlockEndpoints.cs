using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;

namespace EF.Blockchain.Server.Endpoints;

public static class BlockEndpoints
{
    public static void MapBlockEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/blocks/next", (Domain.Blockchain blockchain) =>
        {
            var info = blockchain.GetNextBlock();
            return Results.Json(info);
        });

        app.MapGet("/blocks/{indexOrHash}", (string indexOrHash, Domain.Blockchain blockchain) =>
        {
            Block? block = int.TryParse(indexOrHash, out var index)
                ? blockchain.Blocks.ElementAtOrDefault(index)
                : blockchain.Blocks.FirstOrDefault(b => b.Hash == indexOrHash);

            return block is null ? Results.NotFound() : Results.Ok(block);
        });

        app.MapPost("/blocks", async (HttpContext context, Domain.Blockchain blockchain) =>
        {
            var blockDto = await context.Request.ReadFromJsonAsync<BlockDto>();

            if (blockDto is null || blockDto.Hash is null)
                return Results.UnprocessableEntity("Missing or invalid hash");

            var block = blockDto.ToDomain();

            var result = blockchain.AddBlock(block);

            return result.Success
                ? Results.Created($"/blocks/{block.Index}", block)
                : Results.BadRequest(result);
        });
    }
}
