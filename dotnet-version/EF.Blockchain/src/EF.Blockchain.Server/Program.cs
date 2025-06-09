using EF.Blockchain.Domain;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Blockchain>();

builder.Services.AddLogging();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/// <summary>
/// Log requests similar to morgan('tiny')
/// </summary>
app.Use(async (context, next) =>
{
    var logger = app.Logger;
    var method = context.Request.Method;
    var path = context.Request.Path;

    // Enable buffering so we can read the body
    context.Request.EnableBuffering();

    // Read body as text
    string body = "";
    if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
    {
        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
    }

    var start = DateTime.Now;

    await next();

    var statusCode = context.Response.StatusCode;
    var duration = DateTime.Now - start;

    logger.LogInformation("{Method} {Path} → {StatusCode} ({Duration} ms) | Body: {Body}",
        method, path, statusCode, duration.TotalMilliseconds, body);
});

/// <summary>
/// Blockchain
/// </summary>
var blockchain = app.Services.GetRequiredService<Blockchain>();

app.MapGet("/status", () => new
{
    mempool = blockchain.Mempool.Count,
    blocks = blockchain.Blocks.Count,
    isValid = blockchain.IsValid(),
    lastBlock = blockchain.Blocks.LastOrDefault()
});

app.MapGet("/blocks/next", (Blockchain blockchain) =>
{
    var info = blockchain.GetNextBlock();
    return Results.Json(info);
});

app.MapGet("/blocks/{indexOrHash}", (string indexOrHash) =>
{
    Block? block = int.TryParse(indexOrHash, out var index)
        ? blockchain.Blocks.ElementAtOrDefault(index)
        : blockchain.Blocks.FirstOrDefault(b => b.Hash == indexOrHash);

    return block is null ? Results.NotFound() : Results.Ok(block);
});

app.MapPost("/blocks", async (HttpContext context, Blockchain blockchain) =>
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

app.MapGet("/transactions/{hash?}", (string? hash) =>
{
    if (!string.IsNullOrEmpty(hash))
    {
        var transaction = blockchain.GetTransaction(hash);
        return Results.Json(transaction);
    }

    var next = blockchain.Mempool.Take(Blockchain.TX_PER_BLOCK).ToList();
    var total = blockchain.Mempool.Count;

    return Results.Json(new { next, total });
});

app.MapPost("/transactions", (TransactionDto transactionDto) =>
{
    if (string.IsNullOrEmpty(transactionDto.Hash))
        return Results.StatusCode(422);

    var tx = transactionDto.ToDomain();

    var validation = blockchain.AddTransaction(tx);

    return validation.Success
        ? Results.Created("/transactions", tx)
        : Results.BadRequest(validation);
});

app.Run();

public partial class Program
{ }

public class TransactionDto
{
    public TransactionType? Type { get; set; } = TransactionType.REGULAR;
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = string.Empty;
    public string? Data { get; set; } = string.Empty;

    public Transaction ToDomain()
    {
        return new Transaction(
            type: Type,
            timestamp: Timestamp,
            data: Data
        );
    }
}

public class BlockDto
{
    public int Index { get; set; }
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = null;
    public string PreviousHash { get; set; } = string.Empty;
    public List<TransactionDto> Transactions { get; set; } = new();
    public int? Nonce { get; set; } = null;
    public string? Miner { get; set; } = null;

    public Block ToDomain()
    {
        var transactions = Transactions
            .Select(tx => new Transaction(
                type: tx.Type,
                timestamp: tx.Timestamp,
                data: tx.Data))
            .ToList();
        return new Block(Index, PreviousHash, transactions, Timestamp, Hash, Nonce, Miner);
    }
}
