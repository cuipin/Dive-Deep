using Dive_Deep.ViewModels;

namespace Dive_Deep.Services;

public interface IDiveConditionsService
{
    Task<DiveConditionsLookupResult> GetConditionsAsync(string location, CancellationToken cancellationToken = default);
}

public sealed record DiveConditionsLookupResult(DiveConditionsReport? Report, string? ErrorMessage)
{
    public static DiveConditionsLookupResult Success(DiveConditionsReport report) => new(report, null);
    public static DiveConditionsLookupResult Failure(string message) => new(null, message);
}
