namespace Dive_Deep.Services;

/// <summary>Converts the site's date-time-local form values using Denmark's time zone.</summary>
public static class DanishDateTime
{
    public static TimeZoneInfo TimeZone { get; } =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public static DateTimeOffset ToUtc(DateTime localDateTime)
    {
        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

        if (TimeZone.IsInvalidTime(local))
        {
            throw new ArgumentException("Tidspunktet findes ikke på grund af skift til sommertid.");
        }

        if (TimeZone.IsAmbiguousTime(local))
        {
            throw new ArgumentException("Tidspunktet er tvetydigt på grund af skift fra sommertid.");
        }

        return new DateTimeOffset(local, TimeZone.GetUtcOffset(local)).ToUniversalTime();
    }

    public static DateTime ToLocal(DateTimeOffset utcDateTime) =>
        TimeZoneInfo.ConvertTime(utcDateTime, TimeZone).DateTime;
}
