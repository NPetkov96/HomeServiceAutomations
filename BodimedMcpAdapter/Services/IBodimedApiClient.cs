using BodimedMcpAdapter.Models;

namespace BodimedMcpAdapter.Services;

public interface IBodimedApiClient
{
    Task CreatePatientAsync(
        CreatePatientPayload payload,
        CancellationToken cancellationToken = default);
}
