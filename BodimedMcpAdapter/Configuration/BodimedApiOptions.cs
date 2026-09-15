namespace BodimedMcpAdapter.Configuration;

public sealed class BodimedApiOptions
{
    public const string DefaultBaseUrl =
        "https://home-api.jollyflower-51e4d941.northeurope.azurecontainerapps.io";

    public required Uri BaseUri { get; init; }
    public required string ApiKey { get; init; }

    public static BodimedApiOptions FromConfiguration(
        IConfiguration configuration,
        bool requireHttps = false)
    {
        var apiKey = configuration["BODIMED_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "BODIMED_API_KEY environment variable is required.");
        }

        var baseUrl = configuration["BODIMED_API_BASE_URL"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = DefaultBaseUrl;
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttps && baseUri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                "BODIMED_API_BASE_URL must be an absolute HTTP or HTTPS URL.");
        }

        if (requireHttps && baseUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "BODIMED_API_BASE_URL must use HTTPS in Production.");
        }

        return new BodimedApiOptions
        {
            BaseUri = new Uri(baseUri.ToString().TrimEnd('/') + "/"),
            ApiKey = apiKey
        };
    }
}
