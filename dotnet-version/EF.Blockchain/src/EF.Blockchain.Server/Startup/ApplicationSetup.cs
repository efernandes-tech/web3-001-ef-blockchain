using EF.Blockchain.Domain;
using EF.Blockchain.Server.Middleware;
using Microsoft.AspNetCore.Http.Json;

namespace EF.Blockchain.Server.Startup;

public static class ApplicationSetup
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        var miner = config["Blockchain:MinerWallet:PrivateKey"]
            ?? Environment.GetEnvironmentVariable("BLOCKCHAIN_MINER")
            ?? "default-miner";

        var minerWallet = new Wallet(miner);
        services.AddSingleton<Domain.Blockchain>(_ => new Domain.Blockchain(minerWallet.PublicKey));
        services.Configure<JsonOptions>(options => options.SerializerOptions.WriteIndented = true);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}
