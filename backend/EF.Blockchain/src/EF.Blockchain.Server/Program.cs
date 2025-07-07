using EF.Blockchain.Server.Extensions;
using EF.Blockchain.Server.Startup;
using Serilog;
using System.Diagnostics.CodeAnalysis;

SerilogSetup.ConfigureLogging();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddAppServices(builder.Configuration);

    var app = builder.Build();

    app.UseAppMiddleware();

    app.MapAppEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

[ExcludeFromCodeCoverage]
public partial class Program
{ }
