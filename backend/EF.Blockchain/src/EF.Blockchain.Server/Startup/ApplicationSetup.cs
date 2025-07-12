using System.Reflection;
using EF.Blockchain.Domain;
using EF.Blockchain.Server.Middleware;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;

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

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        services.AddSwaggerGen();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EF Blockchain API",
                Version = "v1",
                Description = "Minimal API for educational blockchain project",
                Contact = new OpenApiContact
                {
                    Name = "Éderson Fernandes",
                    Url = new Uri("https://github.com/efernandes-tech")
                }
            });

            // Enable XML comments if available
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            // Enable enums as strings
            options.UseInlineDefinitionsForEnums();
        });

        return services;
    }

    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        // Enable CORS (must be first)
        app.UseCors("AllowAll");

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}
