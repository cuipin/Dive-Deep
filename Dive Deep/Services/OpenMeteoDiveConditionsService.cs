using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dive_Deep.ViewModels;

namespace Dive_Deep.Services;

public sealed class OpenMeteoDiveConditionsService(HttpClient httpClient) : IDiveConditionsService
{
    private const string GeocodingEndpoint = "https://geocoding-api.open-meteo.com/v1/search";
    private const string ForecastEndpoint = "https://api.open-meteo.com/v1/forecast";
    private const string MarineEndpoint = "https://marine-api.open-meteo.com/v1/marine";
    private static readonly HashSet<int> ThunderstormCodes = [95, 96, 97, 99];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DiveConditionsLookupResult> GetConditionsAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var place = await ResolveLocationAsync(location.Trim(), cancellationToken);
            if (place is null)
            {
                return DiveConditionsLookupResult.Failure(
                    "Vi kunne ikke finde lokationen i Danmark. Prøv et bynavn, postnummer eller koordinater som 55.67, 12.56.");
            }

            var lat = FormatCoordinate(place.Latitude);
            var lon = FormatCoordinate(place.Longitude);
            var forecastUri = new Uri($"{ForecastEndpoint}?latitude={lat}&longitude={lon}&current=wind_speed_10m%2Cprecipitation%2Cweather_code&wind_speed_unit=ms&precipitation_unit=mm&timezone=Europe%2FCopenhagen");
            var marineUri = new Uri($"{MarineEndpoint}?latitude={lat}&longitude={lon}&current=wave_height%2Csea_surface_temperature&cell_selection=sea&timezone=Europe%2FCopenhagen");

            var forecastTask = GetJsonAsync<ForecastResponse>(forecastUri, cancellationToken);
            var marineTask = GetJsonAsync<MarineResponse>(marineUri, cancellationToken);
            await Task.WhenAll(forecastTask, marineTask);

            var weather = forecastTask.Result.Current;
            var marine = marineTask.Result.Current;
            if (weather is null || marine is null
                || weather.WindSpeed is null || weather.Precipitation is null || weather.WeatherCode is null
                || marine.WaveHeight is null || marine.SeaSurfaceTemperature is null)
            {
                return DiveConditionsLookupResult.Failure(
                    "Der mangler vejr- eller havdata for det valgte sted. Prøv koordinater tættere på kysten.");
            }

            if (!DateTime.TryParse(weather.Time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var observationTime))
            {
                observationTime = DateTime.Now;
            }

            var isThunderstorm = ThunderstormCodes.Contains(weather.WeatherCode.Value);
            var reasons = new List<string>();
            if (weather.WindSpeed.Value >= 8)
            {
                reasons.Add($"Vindhastigheden er {weather.WindSpeed.Value:0.0} m/s. Grænsen er under 8 m/s.");
            }

            if (marine.WaveHeight.Value >= 1.5)
            {
                reasons.Add($"Bølgehøjden er {marine.WaveHeight.Value:0.0} m. Grænsen er under 1,5 m.");
            }

            if (weather.Precipitation.Value >= 2)
            {
                reasons.Add($"Nedbøren er {weather.Precipitation.Value:0.0} mm den seneste time. Grænsen er under 2 mm/t.");
            }

            var waterTemperature = marine.SeaSurfaceTemperature.Value;
            var report = new DiveConditionsReport(
                place.Name,
                observationTime,
                place.Latitude,
                place.Longitude,
                weather.WindSpeed.Value,
                marine.WaveHeight.Value,
                weather.Precipitation.Value,
                isThunderstorm,
                waterTemperature,
                reasons.Count == 0 && !isThunderstorm,
                reasons,
                GetWetsuitRecommendation(waterTemperature));

            return DiveConditionsLookupResult.Success(report);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return DiveConditionsLookupResult.Failure(
                "Vejrtjenesten svarede for langsomt. Prøv igen om lidt.");
        }
        catch (HttpRequestException)
        {
            return DiveConditionsLookupResult.Failure(
                "Vi kunne ikke hente data fra vejrtjenesten lige nu. Prøv igen om lidt.");
        }
        catch (JsonException)
        {
            return DiveConditionsLookupResult.Failure(
                "Vejrtjenesten sendte data, vi ikke kunne læse. Prøv igen senere.");
        }
    }

    private async Task<ResolvedLocation?> ResolveLocationAsync(string input, CancellationToken cancellationToken)
    {
        if (TryParseCoordinates(input, out var latitude, out var longitude))
        {
            if (!IsInDenmark(latitude, longitude))
            {
                return null;
            }

            return new ResolvedLocation("Koordinater i Danmark", latitude, longitude);
        }

        var uri = new Uri($"{GeocodingEndpoint}?name={Uri.EscapeDataString(input)}&count=5&language=da&countryCode=DK");
        var response = await GetJsonAsync<GeocodingResponse>(uri, cancellationToken);
        var place = response.Results?.FirstOrDefault(result =>
            string.Equals(result.CountryCode, "DK", StringComparison.OrdinalIgnoreCase));

        if (place is null)
        {
            return null;
        }

        var displayName = string.IsNullOrWhiteSpace(place.Admin1)
            ? place.Name
            : $"{place.Name}, {place.Admin1}";
        return new ResolvedLocation(displayName, place.Latitude, place.Longitude);
    }

    private async Task<T> GetJsonAsync<T>(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new JsonException("The API returned an empty response.");
    }

    private static bool TryParseCoordinates(string input, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;
        var parts = input.Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)
            && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude)
            && double.IsFinite(latitude)
            && double.IsFinite(longitude);
    }

    private static bool IsInDenmark(double latitude, double longitude) =>
        latitude is >= 54.3 and <= 58.0 && longitude is >= 7.7 and <= 15.4;

    private static string FormatCoordinate(double value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string GetWetsuitRecommendation(double waterTemperature) => waterTemperature switch
    {
        > 24 => "Våddragt (3 mm)",
        >= 18 => "Våddragt (5 mm)",
        >= 10 => "Våddragt (7 mm)",
        _ => "Tørdragt"
    };

    private sealed record ResolvedLocation(string Name, double Latitude, double Longitude);

    private sealed class GeocodingResponse
    {
        public List<GeocodingResult>? Results { get; init; }
    }

    private sealed class GeocodingResult
    {
        public string Name { get; init; } = string.Empty;
        public string? Admin1 { get; init; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; init; } = string.Empty;

        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    private sealed class ForecastResponse
    {
        public ForecastCurrent? Current { get; init; }
    }

    private sealed class ForecastCurrent
    {
        public string Time { get; init; } = string.Empty;

        [JsonPropertyName("wind_speed_10m")]
        public double? WindSpeed { get; init; }

        public double? Precipitation { get; init; }

        [JsonPropertyName("weather_code")]
        public int? WeatherCode { get; init; }
    }

    private sealed class MarineResponse
    {
        public MarineCurrent? Current { get; init; }
    }

    private sealed class MarineCurrent
    {
        [JsonPropertyName("wave_height")]
        public double? WaveHeight { get; init; }

        [JsonPropertyName("sea_surface_temperature")]
        public double? SeaSurfaceTemperature { get; init; }
    }
}