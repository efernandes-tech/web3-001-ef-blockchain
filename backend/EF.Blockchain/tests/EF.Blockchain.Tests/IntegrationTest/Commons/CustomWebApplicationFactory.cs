using EF.Blockchain.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Blockchain.Tests.IntegrationTest.Commons;

/// <summary>
/// Custom factory for creating a test server instance of the Web API,
/// allowing injection of a mocked Blockchain instance.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly Domain.Blockchain _mockBlockchain;

    /// <summary>
    /// Initializes the factory with a custom or mocked Blockchain instance.
    /// </summary>
    /// <param name="blockchain">
    /// Optional blockchain instance to inject. If not provided, a default mock with 3 blocks will be used.
    /// </param>
    public CustomWebApplicationFactory(Domain.Blockchain? blockchain = null)
    {
        _mockBlockchain = blockchain ?? BlockchainMockFactory.CreateWithBlocks(3);
    }

    /// <summary>
    /// Configures the WebHost builder to override the Blockchain service
    /// with a mocked or custom instance for integration testing.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing Blockchain service registration
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(Domain.Blockchain));

            if (descriptor != null)
                services.Remove(descriptor);

            // Inject the mock Blockchain
            services.AddSingleton(_mockBlockchain);
        });
    }
}
