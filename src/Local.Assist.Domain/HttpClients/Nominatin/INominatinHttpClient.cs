using Local.Assist.Domain.HttpClients.Nominatin.Responses;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.HttpClients.Nominatin;

public interface INominatinHttpClient
{
    Task<Result<GetGeoInfoResponse[]>> GetGeoInfoAsync(
        string city,
        string state,
        CancellationToken cancellationToken = default);
}
