using BodimedMcpAdapter.Models;
using BodimedMcpAdapter.Services;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BodimedMcpAdapter.Mcp;

[McpServerToolType]
public sealed partial class BodimedMcpTools(IBodimedApiClient apiClient)
{
    [McpServerTool(
        Name = "create_patient",
        Title = "Create a Bodimed patient",
        ReadOnly = false,
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        UseStructuredContent = true)]
    [Authorize(Policy = McpPolicies.Write)]
    [Description("Creates a Bodimed patient by sending personal and medical data to the configured HomeApi. This is a consequential write operation. Show the user every exact field and blood test, then obtain explicit confirmation before setting confirmed=true.")]
    public async Task<CreatePatientResult> CreatePatientAsync(
        [Description("Patient full name. Required.")] string fullName,
        [Description("Patient EGN containing exactly 10 digits. Required.")] string egn,
        [Description("Patient phone number. Required.")] string phoneNumber,
        [Description("Patient date and time in ISO 8601 format, including a UTC offset or Z.")] string date,
        [Description("Final patient note. Required.")] string note,
        [Description("At least one fully specified blood test selected by the user.")]
        IReadOnlyList<BloodTestInput> bloodTests,
        [Description("Must be true only after the user explicitly confirms all exact values.")]
        bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        ValidateRequired(fullName, nameof(fullName));
        ValidateRequired(phoneNumber, nameof(phoneNumber));
        ValidateRequired(note, nameof(note));

        if (egn is null || !EgnRegex().IsMatch(egn))
        {
            throw new McpException("egn must contain exactly 10 digits.");
        }

        if (string.IsNullOrWhiteSpace(date)
            || !IsoDateTimeRegex().IsMatch(date)
            || !DateTimeOffset.TryParse(
                date,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out _))
        {
            throw new McpException(
                "date must be a valid ISO 8601 date-time with a UTC offset or Z.");
        }

        if (bloodTests is null || bloodTests.Count == 0)
        {
            throw new McpException("bloodTests must contain at least one item.");
        }

        if (bloodTests.Any(test => test is null))
        {
            throw new McpException("bloodTests must not contain null items.");
        }

        var duplicateIds = bloodTests
            .GroupBy(test => test.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order()
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new McpException("bloodTests must not contain duplicate IDs.");
        }

        if (bloodTests.Any(test => test.Id <= 0
            || string.IsNullOrWhiteSpace(test.Name)
            || test.BngPrice < 0
            || test.EuroPrice < 0))
        {
            throw new McpException(
                "Every blood test must have a positive ID, a non-empty name, and non-negative prices.");
        }

        if (!confirmed)
        {
            return new CreatePatientResult(
                Created: false,
                ConfirmationRequired: true);
        }

        var payload = new CreatePatientPayload
        {
            Id = 0,
            FullName = fullName.Trim(),
            Egn = egn,
            PhoneNumber = phoneNumber.Trim(),
            Date = date,
            Note = note.Trim(),
            PatientBloodTests = [],
            BloodTests = bloodTests
                .Select(test => test with { Name = test.Name.Trim() })
                .ToArray()
        };

        try
        {
            await apiClient.CreatePatientAsync(payload, cancellationToken);
        }
        catch (BodimedApiException exception)
        {
            throw new McpException(exception.Message);
        }

        return new CreatePatientResult(
            Created: true,
            ConfirmationRequired: false);
    }

    private static void ValidateRequired(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new McpException($"{parameterName} is required.");
        }
    }

    [GeneratedRegex("^[0-9]{10}$", RegexOptions.CultureInvariant)]
    private static partial Regex EgnRegex();

    [GeneratedRegex(
        "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}(?:\\.\\d+)?(?:Z|[+-]\\d{2}:\\d{2})$",
        RegexOptions.CultureInvariant)]
    private static partial Regex IsoDateTimeRegex();
}
