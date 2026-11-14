using System.Text.Json;
using Local.Assist.Domain.Errors;
using Local.Assist.Domain.HttpClients.Nominatim;
using Local.Assist.Domain.HttpClients.Nominatim.Responses;
using Local.Assist.Domain.Shared;

internal sealed class NominatimHttpClient : INominatimHttpClient
{
    private readonly HttpClient _httpClient;

    public NominatimHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<GetGeoInfoResponse[]>> GetGeoInfoAsync(
        string city,
        string state,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"/search?city={Uri.EscapeDataString(city)}&state={Uri.EscapeDataString(state)}&format=json";

        try
        {
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrEmpty(contentString))
                {
                    return Result.Failure<GetGeoInfoResponse[]>(NomatinHttpClientErrors.GetGeoInfoEmptyResponse);
                }

                var content = JsonSerializer.Deserialize<GetGeoInfoResponse[]>(contentString);
                if (content is null)
                {
                    return Result.Failure<GetGeoInfoResponse[]>(NomatinHttpClientErrors.GetGeoInfoDeserializeError);
                }

                return Result.Success(content);
            }

            var error = new Error(response.StatusCode, $"GET_GEO_INFO_{response.StatusCode}", $"Failed to retrieve geo info. Status code: {response.StatusCode}");
            return Result.Failure<GetGeoInfoResponse[]>(error);
        }
        catch (Exception)
        {
            return Result.Failure<GetGeoInfoResponse[]>(NomatinHttpClientErrors.GetGeoInfoUnknownError);
        }
    }
}
