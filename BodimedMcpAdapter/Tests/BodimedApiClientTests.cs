using BodimedMcpAdapter.Configuration;
using BodimedMcpAdapter.Models;
using BodimedMcpAdapter.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Nodes;

namespace BodimedMcpAdapter.Tests;

public sealed class BodimedApiClientTests
{
    [Fact]
    public async Task SendsCorrectUrlHeaderAndExactPayload()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var client = CreateClient(handler);
        var payload = ValidPayload();

        await client.CreatePatientAsync(payload);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            new Uri(BodimedApiOptions.DefaultBaseUrl + "/api/Bodimed/createPatient"),
            handler.RequestUri);
        Assert.Equal("test-api-key", handler.ApiKey);

        var expected = JsonNode.Parse("""
            {
              "id": 0,
              "fullName": "Test Patient",
              "egn": "1234567890",
              "phoneNumber": "+359000000000",
              "date": "2026-09-15T10:00:00+03:00",
              "note": "Test note",
              "patientBloodTests": [],
              "bloodTests": [
                {
                  "id": 17,
                  "name": "TSH",
                  "bngPrice": 10.20,
                  "euroPrice": 5.21,
                  "hasPriority": true
                }
              ]
            }
            """);
        var actual = JsonNode.Parse(handler.Body!);

        Assert.True(JsonNode.DeepEquals(expected, actual));
    }

    [Fact]
    public async Task ApiError_ThrowsWithoutReadingResponseBody()
    {
        var handler = new RecordingHandler(
            HttpStatusCode.InternalServerError,
            "response may contain sensitive server details");
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<BodimedApiException>(() =>
            client.CreatePatientAsync(ValidPayload()));

        Assert.Equal(500, exception.StatusCode);
        Assert.DoesNotContain("sensitive", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Timeout_ReportsUnknownOutcomeWithoutRetryingOrLoggingSensitiveData()
    {
        var logger = new CollectingLogger<BodimedApiClient>();
        var handler = new TimeoutHandler();
        var client = CreateClient(handler, logger, "secret-api-key-value");
        var payload = SensitivePayload();

        var exception = await Assert.ThrowsAsync<BodimedApiException>(() =>
            client.CreatePatientAsync(payload));

        Assert.Equal(1, handler.CallCount);
        Assert.Null(exception.StatusCode);
        Assert.Contains("outcome is unknown", exception.Message, StringComparison.OrdinalIgnoreCase);

        var logs = string.Join(Environment.NewLine, logger.Messages);
        Assert.Contains("outcome is unknown", logs, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(payload.FullName, logs);
        Assert.DoesNotContain(payload.Egn, logs);
        Assert.DoesNotContain(payload.PhoneNumber, logs);
        Assert.DoesNotContain(payload.Note, logs);
        Assert.DoesNotContain(payload.BloodTests[0].Name, logs);
        Assert.DoesNotContain("secret-api-key-value", logs);
    }

    [Fact]
    public async Task CallerCancellation_IsNotReportedAsApiTimeout()
    {
        var logger = new CollectingLogger<BodimedApiClient>();
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var client = CreateClient(handler, logger);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.CreatePatientAsync(ValidPayload(), cancellation.Token));

        Assert.DoesNotContain(
            logger.Messages,
            message => message.Contains("outcome is unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MissingApiKeyEnvironmentVariable_IsRejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BODIMED_API_BASE_URL"] = BodimedApiOptions.DefaultBaseUrl
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BodimedApiOptions.FromConfiguration(configuration));

        Assert.Contains("BODIMED_API_KEY", exception.Message);
    }

    [Fact]
    public async Task LogsDoNotContainSensitiveValues()
    {
        const string apiKey = "secret-api-key-value";
        var logger = new CollectingLogger<BodimedApiClient>();
        var handler = new RecordingHandler(HttpStatusCode.BadRequest);
        var client = CreateClient(handler, logger, apiKey);
        var payload = SensitivePayload();

        await Assert.ThrowsAsync<BodimedApiException>(() =>
            client.CreatePatientAsync(payload));

        var logs = string.Join(Environment.NewLine, logger.Messages);
        Assert.DoesNotContain(payload.FullName, logs);
        Assert.DoesNotContain(payload.Egn, logs);
        Assert.DoesNotContain(payload.PhoneNumber, logs);
        Assert.DoesNotContain(payload.Note, logs);
        Assert.DoesNotContain(payload.BloodTests[0].Name, logs);
        Assert.DoesNotContain(apiKey, logs);
    }

    private static BodimedApiClient CreateClient(
        HttpMessageHandler handler,
        ILogger<BodimedApiClient>? logger = null,
        string apiKey = "test-api-key")
    {
        var options = new BodimedApiOptions
        {
            BaseUri = new Uri(BodimedApiOptions.DefaultBaseUrl + "/"),
            ApiKey = apiKey
        };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = options.BaseUri
        };

        return new BodimedApiClient(
            httpClient,
            options,
            logger ?? new CollectingLogger<BodimedApiClient>());
    }

    private static CreatePatientPayload ValidPayload() => new()
    {
        Id = 0,
        FullName = "Test Patient",
        Egn = "1234567890",
        PhoneNumber = "+359000000000",
        Date = "2026-09-15T10:00:00+03:00",
        Note = "Test note",
        PatientBloodTests = [],
        BloodTests = [new BloodTestInput(17, "TSH", 10.20m, 5.21m, true)]
    };

    private static CreatePatientPayload SensitivePayload() => new()
    {
        Id = 0,
        FullName = "Sensitive Person",
        Egn = "9876543210",
        PhoneNumber = "+359888111222",
        Date = "2026-09-15T10:00:00+03:00",
        Note = "Sensitive medical note",
        PatientBloodTests = [],
        BloodTests = [new BloodTestInput(17, "Sensitive blood test", 10.20m, 5.21m, true)]
    };

    private sealed class RecordingHandler(
        HttpStatusCode statusCode,
        string responseBody = "") : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues("X-Api-Key", out var values)
                ? values.Single()
                : null;
            Body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody)
            };
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new TaskCanceledException("Simulated HttpClient timeout.");
        }
    }

    private sealed class CollectingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
