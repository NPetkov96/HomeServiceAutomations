using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BodimedMcpAdapter.Models;

public sealed record BloodTestInput(
    [property: JsonPropertyName("id"), Description("Positive Bodimed blood-test ID.")]
    int Id,
    [property: JsonPropertyName("name"), Description("Exact blood-test name selected by the user.")]
    string Name,
    [property: JsonPropertyName("bngPrice"), Description("Non-negative price in BGN.")]
    decimal BngPrice,
    [property: JsonPropertyName("euroPrice"), Description("Non-negative price in EUR.")]
    decimal EuroPrice,
    [property: JsonPropertyName("hasPriority"), Description("Whether the test has priority.")]
    bool HasPriority);

public sealed record CreatePatientResult(
    [property: JsonPropertyName("created")]
    bool Created,
    [property: JsonPropertyName("confirmationRequired")]
    bool ConfirmationRequired);

public sealed class CreatePatientPayload
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("fullName")]
    public required string FullName { get; init; }

    [JsonPropertyName("egn")]
    public required string Egn { get; init; }

    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; init; }

    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("note")]
    public required string Note { get; init; }

    [JsonPropertyName("patientBloodTests")]
    public IReadOnlyList<object> PatientBloodTests { get; init; } = [];

    [JsonPropertyName("bloodTests")]
    public required IReadOnlyList<BloodTestInput> BloodTests { get; init; }
}
