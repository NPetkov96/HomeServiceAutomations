using BodimedMcpAdapter.Mcp;
using BodimedMcpAdapter.Models;
using BodimedMcpAdapter.Services;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BodimedMcpAdapter.Tests;

public sealed class BodimedMcpToolsTests
{
    [Fact]
    public void ToolSchema_ContainsOnlyTheRequestedInputs()
    {
        var exposedTools = typeof(BodimedMcpTools)
            .GetMethods()
            .Where(method => method.GetCustomAttributes(
                typeof(McpServerToolAttribute),
                inherit: true).Length > 0)
            .ToArray();
        var instance = new BodimedMcpTools(new RecordingApiClient());
        var tool = McpServerTool.Create(
            Assert.Single(exposedTools),
            instance,
            new McpServerToolCreateOptions());
        var schema = tool.ProtocolTool.InputSchema;
        var properties = schema.GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .Order()
            .ToArray();

        Assert.Equal(
            new[]
            {
                "bloodTests",
                "confirmed",
                "date",
                "egn",
                "fullName",
                "note",
                "phoneNumber"
            },
            properties);
        Assert.Equal("create_patient", tool.ProtocolTool.Name);
        Assert.False(tool.ProtocolTool.Annotations!.ReadOnlyHint);
        Assert.True(tool.ProtocolTool.Annotations.DestructiveHint);
        Assert.False(tool.ProtocolTool.Annotations.IdempotentHint);
        var authorization = Assert.Single(
            Assert.Single(exposedTools).GetCustomAttributes(
                typeof(AuthorizeAttribute),
                inherit: true).Cast<AuthorizeAttribute>());
        Assert.Equal(McpPolicies.Write, authorization.Policy);
    }

    [Fact]
    public async Task ConfirmedFalse_DoesNotSendPost()
    {
        var apiClient = new RecordingApiClient();
        var tool = new BodimedMcpTools(apiClient);

        var result = await InvokeValidAsync(tool, confirmed: false);

        Assert.False(result.Created);
        Assert.True(result.ConfirmationRequired);
        Assert.Equal(0, apiClient.CallCount);
    }

    [Fact]
    public async Task ConfirmedTrue_SendsExactlyOnePost()
    {
        var apiClient = new RecordingApiClient();
        var tool = new BodimedMcpTools(apiClient);

        var result = await InvokeValidAsync(tool, confirmed: true);

        Assert.True(result.Created);
        Assert.False(result.ConfirmationRequired);
        Assert.Equal(1, apiClient.CallCount);
        Assert.NotNull(apiClient.LastPayload);
        Assert.Equal(0, apiClient.LastPayload.Id);
        Assert.Equal("Test Patient", apiClient.LastPayload.FullName);
        Assert.Equal("1234567890", apiClient.LastPayload.Egn);
        Assert.Equal("+359000000000", apiClient.LastPayload.PhoneNumber);
        Assert.Equal("2026-09-15T10:00:00+03:00", apiClient.LastPayload.Date);
        Assert.Equal("Test note", apiClient.LastPayload.Note);
        Assert.Empty(apiClient.LastPayload.PatientBloodTests);
        Assert.Equal(ValidBloodTest(), Assert.Single(apiClient.LastPayload.BloodTests));
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("12345A7890")]
    public async Task InvalidEgn_IsRejected(string egn)
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            egn,
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [ValidBloodTest()],
            confirmed: true));
    }

    [Theory]
    [InlineData("15.09.2026 10:00")]
    [InlineData("2026-09-15T10:00:00")]
    [InlineData("not-a-date")]
    public async Task InvalidDate_IsRejected(string date)
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            date,
            "Test note",
            [ValidBloodTest()],
            confirmed: true));
    }

    [Fact]
    public async Task EmptyBloodTests_AreRejected()
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [],
            confirmed: true));
    }

    [Fact]
    public async Task DuplicateBloodTestIds_AreRejected()
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [ValidBloodTest(), ValidBloodTest() with { Name = "Another test" }],
            confirmed: true));
    }

    [Fact]
    public async Task NullBloodTest_IsRejected()
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [null!],
            confirmed: true));
    }

    [Theory]
    [MemberData(nameof(InvalidBloodTests))]
    public async Task InvalidBloodTest_IsRejected(BloodTestInput bloodTest)
    {
        var tool = new BodimedMcpTools(new RecordingApiClient());

        await Assert.ThrowsAsync<McpException>(() => tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [bloodTest],
            confirmed: true));
    }

    public static TheoryData<BloodTestInput> InvalidBloodTests => new()
    {
        new BloodTestInput(0, "TSH", 10.20m, 5.21m, false),
        new BloodTestInput(1, " ", 10.20m, 5.21m, false),
        new BloodTestInput(1, "TSH", -1m, 5.21m, false),
        new BloodTestInput(1, "TSH", 10.20m, -1m, false)
    };

    private static Task<CreatePatientResult> InvokeValidAsync(
        BodimedMcpTools tool,
        bool confirmed)
    {
        return tool.CreatePatientAsync(
            "Test Patient",
            "1234567890",
            "+359000000000",
            "2026-09-15T10:00:00+03:00",
            "Test note",
            [ValidBloodTest()],
            confirmed);
    }

    private static BloodTestInput ValidBloodTest() =>
        new(17, "TSH", 10.20m, 5.21m, true);

    private sealed class RecordingApiClient : IBodimedApiClient
    {
        public int CallCount { get; private set; }
        public CreatePatientPayload? LastPayload { get; private set; }

        public Task CreatePatientAsync(
            CreatePatientPayload payload,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPayload = payload;
            return Task.CompletedTask;
        }
    }
}
