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
    numberOfBlocks = blockchain.Blocks.Count,
    isValid = blockchain.IsValid(),
    lastBlock = blockchain.Blocks.LastOrDefault()
});

app.MapGet("/blocks/{indexOrHash}", (string indexOrHash) =>
{
    Block? block = int.TryParse(indexOrHash, out var index)
        ? blockchain.Blocks.ElementAtOrDefault(index)
        : blockchain.Blocks.FirstOrDefault(b => b.Hash == indexOrHash);

    return block is null ? Results.NotFound() : Results.Ok(block);
});

app.Run();
