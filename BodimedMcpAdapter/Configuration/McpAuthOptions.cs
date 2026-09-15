namespace BodimedMcpAdapter.Configuration;

public sealed class McpAuthOptions
{
    public required Uri Issuer { get; init; }
    public required string Audience { get; init; }
    public required Uri AuthorizationServer { get; init; }
    public required Uri PublicBaseUri { get; init; }
    public required string RequiredScope { get; init; }

    public static McpAuthOptions FromConfiguration(
        IConfiguration configuration,
        bool requireHttps)
    {
        var errors = new List<string>();
        var issuer = ReadUri(configuration, "MCP_AUTH_ISSUER", requireHttps, errors);
        var authorizationServer = ReadUri(
            configuration,
            "MCP_AUTH_AUTHORIZATION_SERVER",
            requireHttps,
            errors);
        var publicBaseUri = ReadUri(
            configuration,
            "MCP_AUTH_PUBLIC_BASE_URL",
            requireHttps,
            errors);
        var audience = ReadRequired(configuration, "MCP_AUTH_AUDIENCE", errors);
        var requiredScope = ReadRequired(configuration, "MCP_AUTH_REQUIRED_SCOPE", errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"MCP OAuth configuration is incomplete: {string.Join(" ", errors)}");
        }

        return new McpAuthOptions
        {
            Issuer = issuer!,
            Audience = audience!,
            AuthorizationServer = authorizationServer!,
            PublicBaseUri = new Uri(publicBaseUri!.ToString().TrimEnd('/') + "/"),
            RequiredScope = requiredScope!
        };
    }

    private static string? ReadRequired(
        IConfiguration configuration,
        string name,
        ICollection<string> errors)
    {
        var value = configuration[name];
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{name} is required.");
            return null;
        }

        return value;
    }

    private static Uri? ReadUri(
        IConfiguration configuration,
        string name,
        bool requireHttps,
        ICollection<string> errors)
    {
        var value = ReadRequired(configuration, name, errors);
        if (value is null)
        {
            return null;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            errors.Add($"{name} must be an absolute URL.");
            return null;
        }

        if (requireHttps && uri.Scheme != Uri.UriSchemeHttps)
        {
            errors.Add($"{name} must use HTTPS in Production.");
        }

        return uri;
    }
}
