using System.Text.Json.Serialization;

namespace Local.Assist.Domain.HttpClients.OpenMeteo.Responses;

public sealed record GetWeatherForecastResponse(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("timezone_abbreviation")] string TimezoneAbbreviation,
    [property: JsonPropertyName("hourly_units")] GetWeatherForecastResponseHourlyUnits HourlyUnits,
    [property: JsonPropertyName("hourly")] GetWeatherForecastResponseHourly Hourly);

public sealed record GetWeatherForecastResponseHourlyUnits(
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("temperature_2m")] string Temperature2m,
    [property: JsonPropertyName("precipitation_probability")] string PrecipitationProbability,
    [property: JsonPropertyName("precipitation")] string Precipitation);

public sealed record GetWeatherForecastResponseHourly(
    [property: JsonPropertyName("time")] DateTime[] Time,
    [property: JsonPropertyName("temperature_2m")] double[] Temperature2m,
    [property: JsonPropertyName("precipitation_probability")] double[] PrecipitationProbability,
    [property: JsonPropertyName("precipitation")] double[] Precipitation,
    [property: JsonPropertyName("weather_code")] int[] WeatherCode);

