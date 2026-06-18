using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Integration;

internal static class TestApp
{
    private static GifPage DefaultPage(int offset) => new(
        [new GifItem("id1", "https://media.giphy.com/test.gif")],
        offset, 25, 1, 100);

    public static (WebApplicationFactory<Program> factory, Mock<IGifProvider> providerMock) Create()
    {
        var providerMock = new Mock<IGifProvider>();
        providerMock
            .Setup(p => p.GetTrendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int offset, CancellationToken ct) => DefaultPage(offset));
        providerMock
            .Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string term, int offset, CancellationToken ct) => DefaultPage(offset));

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGifProvider));
                if (descriptor is not null)
                    services.Remove(descriptor);

                var httpDescriptors = services
                    .Where(d => d.ImplementationType?.Name == "GiphyProvider"
                             || (d.ImplementationFactory is not null && d.ServiceType == typeof(IGifProvider)))
                    .ToList();
                foreach (var d in httpDescriptors) services.Remove(d);

                services.AddSingleton<IGifProvider>(providerMock.Object);
            });

            builder.UseSetting("Giphy:ApiKey", "test-key");
            builder.UseSetting("Giphy:BaseUrl", "https://api.giphy.com");
        });

        return (factory, providerMock);
    }
}
