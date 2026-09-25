namespace Dive_Deep.Services.Contracts;

public sealed record BookingLineRequest(
    int ProductVariantId,
    int Quantity,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
