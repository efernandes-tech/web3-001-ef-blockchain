using Serilog;

namespace EF.Blockchain.Server.Startup;

public static class SerilogSetup
{
    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
    }
}
