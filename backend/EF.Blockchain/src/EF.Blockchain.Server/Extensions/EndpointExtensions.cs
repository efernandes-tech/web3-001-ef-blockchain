using EF.Blockchain.Server.Endpoints;

namespace EF.Blockchain.Server.Extensions;

public static class EndpointExtensions
{
    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapStatusEndpoints();
        app.MapBlockEndpoints();
        app.MapTransactionEndpoints();
        app.MapWalletEndpoints();
    }
}
