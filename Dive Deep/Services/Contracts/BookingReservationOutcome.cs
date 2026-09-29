namespace Dive_Deep.Services.Contracts;

public enum BookingReservationOutcome
{
    Reserved,
    Unavailable,
    ProductVariantNotFound,
    BookingNotFound,
    ConcurrencyConflict
}
