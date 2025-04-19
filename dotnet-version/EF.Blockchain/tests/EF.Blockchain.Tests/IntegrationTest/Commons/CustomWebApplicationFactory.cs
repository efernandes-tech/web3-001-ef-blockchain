using EF.Blockchain.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Blockchain.Tests.IntegrationTest.Commons;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Domain.Blockchain _mockBlockchain;

    public CustomWebApplicationFactory()
    {
        _mockBlockchain = BlockchainMockFactory.CreateWithBlocks(3);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(Domain.Blockchain));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton(_mockBlockchain);
        });
    }
}
