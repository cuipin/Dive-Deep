namespace Dive_Deep.Services.Contracts;

public sealed record BookingOperationResult(
    bool Succeeded,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int? BookingId = null)
{
    public static BookingOperationResult Success(int bookingId) =>
        new(true, BookingId: bookingId);

    public static BookingOperationResult Failure(string code, string message) =>
        new(false, code, message);
}
