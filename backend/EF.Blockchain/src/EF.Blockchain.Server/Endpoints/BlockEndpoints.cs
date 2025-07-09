using EF.Blockchain.Domain;
using EF.Blockchain.Server.Dtos;
using EF.Blockchain.Server.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EF.Blockchain.Server.Endpoints;

/// <summary>
/// Provides endpoints for accessing and submitting blocks in the blockchain.
/// </summary>
public static class BlockEndpoints
{
    /// <summary>
    /// Maps the block-related endpoints to the application.
    /// </summary>
    /// <param name="app">The route builder to register endpoints.</param>
    public static void MapBlockEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/blocks/next", GetNextBlock)
            .WithName("GetNextBlock")
            .WithTags("Block")
            .WithSummary("Get info for the next block to be mined")
            .WithDescription("Returns the data needed for mining the next block, including transactions and metadata.")
            .Produces<BlockInfoDto>(StatusCodes.Status200OK)
            .WithOpenApi();

        app.MapGet("/blocks/{indexOrHash}", GetBlockByIndexOrHash)
            .WithName("GetBlockByIndexOrHash")
            .WithTags("Block")
            .WithSummary("Get block by index or hash")
            .WithDescription("Returns a block from the chain by its index or hash.")
            .Produces<BlockDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapPost("/blocks", PostBlock)
            .WithName("PostBlock")
            .WithTags("Block")
            .WithSummary("Add a new block to the blockchain")
            .WithDescription("Receives a mined block and attempts to add it to the blockchain.")
            .Accepts<BlockDto>("application/json")
            .Produces<BlockDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .WithOpenApi();
    }

    /// <summary>
    /// Handles GET /blocks/next
    /// </summary>
    /// <param name="blockchain">Injected blockchain instance.</param>
    /// <returns>Returns the next block to be mined.</returns>
    private static IResult GetNextBlock([FromServices] Domain.Blockchain blockchain)
    {
        var info = blockchain.GetNextBlock();
        return Results.Ok(BlockInfoMapper.ToDto(info));
    }

    /// <summary>
    /// Handles GET /blocks/{indexOrHash}
    /// </summary>
    /// <param name="indexOrHash">Block index (int) or hash (string).</param>
    /// <param name="blockchain">Injected blockchain instance.</param>
    /// <returns>Returns the block if found, or 404 if not.</returns>
    private static IResult GetBlockByIndexOrHash(
        [FromRoute] string indexOrHash,
        [FromServices] Domain.Blockchain blockchain)
    {
        Block? block = int.TryParse(indexOrHash, out var index)
            ? blockchain.Blocks.ElementAtOrDefault(index)
            : blockchain.Blocks.FirstOrDefault(b => b.Hash == indexOrHash);

        return block is null ? Results.NotFound() : Results.Ok(BlockMapper.ToDto(block));
    }

    /// <summary>
    /// Handles POST /blocks
    /// </summary>
    /// <param name="context">HTTP context to read request body.</param>
    /// <param name="blockchain">Injected blockchain instance.</param>
    /// <returns>Returns 201 if block is added, otherwise error code.</returns>
    private static async Task<IResult> PostBlock(
        HttpContext context,
        [FromServices] Domain.Blockchain blockchain)
    {
        var blockDto = await context.Request.ReadFromJsonAsync<BlockDto>();

        if (blockDto is null || string.IsNullOrEmpty(blockDto.Hash))
            return Results.UnprocessableEntity("Missing or invalid hash");

        var block = BlockMapper.ToDomain(blockDto);
        var result = blockchain.AddBlock(block);

        return result.Success
            ? Results.Created($"/blocks/{block.Index}", BlockMapper.ToDto(block))
            : Results.BadRequest(result);
    }
}
