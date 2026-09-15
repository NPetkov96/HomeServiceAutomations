namespace BodimedMcpAdapter.Services;

public sealed class BodimedApiException : Exception
{
    public BodimedApiException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
