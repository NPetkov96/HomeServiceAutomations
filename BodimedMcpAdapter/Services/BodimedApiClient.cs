using BodimedMcpAdapter.Configuration;
using BodimedMcpAdapter.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BodimedMcpAdapter.Services;

public sealed class BodimedApiClient(
    HttpClient httpClient,
    BodimedApiOptions options,
    ILogger<BodimedApiClient> logger) : IBodimedApiClient
{
    private const string CreatePatientPath = "api/Bodimed/createPatient";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task CreatePatientAsync(
        CreatePatientPayload payload,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, CreatePatientPath);
        request.Headers.TryAddWithoutValidation("X-Api-Key", options.ApiKey);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Bodimed create-patient request failed with HTTP status {StatusCode}.",
                    (int)response.StatusCode);
                throw new BodimedApiException(
                    "Bodimed API rejected the create-patient request.",
                    (int)response.StatusCode);
            }
        }
        catch (BodimedApiException)
        {
            throw;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Bodimed create-patient request timed out; its outcome is unknown.");
            throw new BodimedApiException(
                "Bodimed API response timed out; the operation outcome is unknown. Check patient history before retrying.",
                null,
                exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning("Bodimed create-patient request could not be completed.");
            throw new BodimedApiException(
                "Bodimed API could not be reached.",
                exception.StatusCode is null ? null : (int)exception.StatusCode,
                exception);
        }
    }
}
