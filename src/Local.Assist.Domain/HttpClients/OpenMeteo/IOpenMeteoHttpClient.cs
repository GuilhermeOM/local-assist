using Local.Assist.Domain.HttpClients.OpenMeteo.Responses;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.HttpClients.OpenMeteo;

public interface IOpenMeteoHttpClient
{
    Task<Result<GetWeatherForecastResponse>> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
