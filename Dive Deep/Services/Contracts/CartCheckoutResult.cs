namespace Dive_Deep.Services.Contracts;

public sealed record CartCheckoutResult(
    bool Succeeded,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int? BookingId = null)
{
    public static CartCheckoutResult Success(int bookingId) => new(true, BookingId: bookingId);

    public static CartCheckoutResult Failure(string code, string message) => new(false, code, message);
}
