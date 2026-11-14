using Local.Assist.Domain.HttpClients.Nomatin.Responses;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.HttpClients.Nomatin;

public interface INomatinHttpClient
{
    Task<Result<GetGeoInfoResponse[]>> GetGeoInfoAsync(
        string city,
        string state,
        CancellationToken cancellationToken = default);
}
