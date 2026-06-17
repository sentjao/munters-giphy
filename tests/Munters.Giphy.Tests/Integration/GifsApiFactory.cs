using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Integration;

public sealed class GifsApiFactory : WebApplicationFactory<Program>
{
    public FakeGifProvider FakeProvider { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the real IGifProvider with the fake
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IGifProvider));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Also remove any typed HttpClient registration that registers GiphyProvider
            var httpDescriptors = services
                .Where(d => d.ImplementationType?.Name == "GiphyProvider"
                         || (d.ImplementationFactory is not null && d.ServiceType == typeof(IGifProvider)))
                .ToList();
            foreach (var d in httpDescriptors) services.Remove(d);

            services.AddSingleton<IGifProvider>(FakeProvider);
        });

        builder.UseSetting("Giphy:ApiKey", "test-key");
        builder.UseSetting("Giphy:BaseUrl", "https://api.giphy.com");
    }
}
