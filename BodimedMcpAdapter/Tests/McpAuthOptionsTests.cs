using BodimedMcpAdapter.Configuration;
using Microsoft.Extensions.Configuration;

namespace BodimedMcpAdapter.Tests;

public sealed class McpAuthOptionsTests
{
    [Fact]
    public void MissingOAuthEnvironmentVariables_AreRejected()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            McpAuthOptions.FromConfiguration(configuration, requireHttps: true));

        Assert.Contains("MCP_AUTH_ISSUER", exception.Message);
        Assert.Contains("MCP_AUTH_AUDIENCE", exception.Message);
        Assert.Contains("MCP_AUTH_AUTHORIZATION_SERVER", exception.Message);
        Assert.Contains("MCP_AUTH_PUBLIC_BASE_URL", exception.Message);
        Assert.Contains("MCP_AUTH_REQUIRED_SCOPE", exception.Message);
    }

    [Fact]
    public void ProductionOAuthUrls_MustUseHttps()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MCP_AUTH_ISSUER"] = "http://issuer.test",
                ["MCP_AUTH_AUDIENCE"] = "bodimed-mcp",
                ["MCP_AUTH_AUTHORIZATION_SERVER"] = "http://auth.test",
                ["MCP_AUTH_PUBLIC_BASE_URL"] = "http://mcp.test",
                ["MCP_AUTH_REQUIRED_SCOPE"] = "bodimed.write"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            McpAuthOptions.FromConfiguration(configuration, requireHttps: true));

        Assert.Contains("must use HTTPS", exception.Message);
    }
}
