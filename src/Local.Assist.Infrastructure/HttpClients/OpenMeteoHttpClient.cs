using System.Net.Http.Json;
using System.Globalization;
using Local.Assist.Domain.Errors;
using Local.Assist.Domain.HttpClients.OpenMeteo;
using Local.Assist.Domain.HttpClients.OpenMeteo.Responses;
using Local.Assist.Domain.Shared;

internal sealed class OpenMeteoHttpClient : IOpenMeteoHttpClient
{
    private readonly HttpClient _httpClient;

    public OpenMeteoHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<GetWeatherForecastResponse>> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var latitudeValue = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeValue = longitude.ToString(CultureInfo.InvariantCulture);
        var dateValue = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var endpoint = $"v1/forecast?latitude={latitudeValue}&longitude={longitudeValue}&hourly=temperature_2m,precipitation_probability,precipitation,weather_code&start_date={dateValue}&end_date={dateValue}&timezone=auto";

        try
        {
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<GetWeatherForecastResponse>(cancellationToken);
                if (content is null)
                {
                    return Result.Failure<GetWeatherForecastResponse>(OpenMeteoHttpClientErrors.GetWeatherForecastDeserializeError);
                }

                return Result.Success(content);
            }

            var error = new Error(response.StatusCode, $"GET_WEATHER_FORECAST_{response.StatusCode}", $"Failed to retrieve weather forecast. Status code: {response.StatusCode}");
            return Result.Failure<GetWeatherForecastResponse>(error);
        }
        catch (Exception)
        {
            return Result.Failure<GetWeatherForecastResponse>(OpenMeteoHttpClientErrors.GetWeatherForecastUnknownError);
        }
    }
}
