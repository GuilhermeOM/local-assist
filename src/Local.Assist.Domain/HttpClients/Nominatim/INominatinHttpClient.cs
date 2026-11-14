using Local.Assist.Domain.HttpClients.Nominatim.Responses;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.HttpClients.Nominatim;

public interface INominatimHttpClient
{
    Task<Result<GetGeoInfoResponse[]>> GetGeoInfoAsync(
        string city,
        string state,
        CancellationToken cancellationToken = default);
}
