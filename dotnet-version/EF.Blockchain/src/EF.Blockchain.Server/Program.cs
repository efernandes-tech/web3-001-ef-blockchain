using EF.Blockchain.Domain;
using Microsoft.AspNetCore.Http.Json;
using Serilog;
using System.Diagnostics.CodeAnalysis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var miner = builder.Configuration["Blockchain:MinerWallet:PrivateKey"]
    ?? Environment.GetEnvironmentVariable("BLOCKCHAIN_MINER")
    ?? "default-miner";

var minerWallet = new Wallet(miner);

builder.Services.AddSingleton<Blockchain>(_ => new Blockchain(minerWallet.PublicKey));

builder.Host.UseSerilog();

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

    Log.Information("{Method} {Path} → {StatusCode} ({Duration} ms) | Body: {Body}",
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

app.MapGet("/wallets/{walletAddress}", (string walletAddress) =>
{
    // TODO: make real UTXO logic
    var dummyUtxo = new TransactionOutput(
        toAddress: walletAddress,
        amount: 10,
        tx: "abc"
    );

    return Results.Json(new
    {
        balance = 10,
        fee = blockchain.GetFeePerTx(),
        utxo = new List<TransactionOutput> { dummyUtxo }
    });
});

AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{ }

[ExcludeFromCodeCoverage]
public class TransactionInputDto
{
    public string? FromAddress { get; set; } = string.Empty;
    public long? Amount { get; set; } = null;
    public string? Signature { get; set; } = string.Empty;
    public string? PreviousTx { get; set; } = string.Empty;

    public TransactionInput ToDomain()
    {
        return new TransactionInput(FromAddress, Amount, Signature, PreviousTx);
    }
}

[ExcludeFromCodeCoverage]
public class TransactionOutputDto
{
    public string? ToAddress { get; set; } = string.Empty;
    public long? Amount { get; set; } = 0;
    public string? Tx { get; set; } = string.Empty;

    public TransactionOutput ToDomain()
    {
        return new TransactionOutput(ToAddress, (int)(Amount ?? 0), Tx);
    }
}

[ExcludeFromCodeCoverage]
public class TransactionDto
{
    public TransactionType? Type { get; set; } = TransactionType.REGULAR;
    public long? Timestamp { get; set; } = null;
    public string? Hash { get; set; } = string.Empty;
    public List<TransactionInputDto>? TxInputs { get; set; } = null;
    public List<TransactionOutputDto> TxOutputs { get; set; } = new();

    public Transaction ToDomain()
    {
        var txInputs = TxInputs?.Select(i => i.ToDomain()).ToList();
        var txOutputs = TxOutputs.Select(o => o.ToDomain()).ToList();

        return new Transaction(
            type: Type,
            timestamp: Timestamp,
            txInputs: txInputs,
            txOutputs: txOutputs);
    }
}

[ExcludeFromCodeCoverage]
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
            .Select(tx => tx.ToDomain())
            .ToList();

        return new Block(Index, PreviousHash, transactions, Timestamp, Hash, Nonce, Miner);
    }
}
