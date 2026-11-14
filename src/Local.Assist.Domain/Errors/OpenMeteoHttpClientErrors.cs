using System.Net;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.Errors;

public static class OpenMeteoHttpClientErrors
{
    public static Error GetWeatherForecastDeserializeError => new(HttpStatusCode.InternalServerError, "GET_WEATHER_FORECAST_DESERIALIZE_ERROR", "The response content could not be deserialized.");
    public static Error GetWeatherForecastUnknownError => new(HttpStatusCode.InternalServerError, "GET_WEATHER_FORECAST_UNKNOWN_ERROR", "An unknown error occurred while retrieving the weather forecast.");
}
