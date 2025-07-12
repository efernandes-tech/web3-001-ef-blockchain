using EF.Blockchain.Server.Dtos;
using EF.Blockchain.Server.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace EF.Blockchain.Server.Endpoints;

/// <summary>
/// Provides blockchain status-related endpoints.
/// </summary>
public static class StatusEndpoints
{
    /// <summary>
    /// Maps the status-related endpoints to the application's route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/status", GetBlockchainStatus)
            .WithName("GetBlockchainStatus")
            .WithTags("Status")
            .WithSummary("Get blockchain status")
            .WithDescription("Returns mempool size, block count, validation status, and latest block.")
            .Produces<StatusDto>(statusCode: StatusCodes.Status200OK)
            .WithOpenApi();
    }

    /// <summary>
    /// Handles the GET /status endpoint.
    /// </summary>
    /// <param name="blockchain">The blockchain instance.</param>
    /// <returns>Blockchain status information.</returns>
    private static StatusDto GetBlockchainStatus([FromServices] Domain.Blockchain blockchain)
    {
        var lastBlock = blockchain.Blocks.LastOrDefault();

        return new StatusDto
        {
            Mempool = blockchain.Mempool.Count,
            Blocks = blockchain.Blocks.Count,
            IsValid = blockchain.IsValid().Success,
            LastBlock = lastBlock is null
                ? null
                : BlockMapper.ToDto(lastBlock),
            Difficulty = blockchain.GetDifficulty()
        };
    }
}
