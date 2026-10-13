using System.Net;
using Store.Api.Tests.Infrastructure;

namespace Store.Api.Tests;

public class HealthCheckTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Health_endpoint_reports_healthy_when_database_is_reachable()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
