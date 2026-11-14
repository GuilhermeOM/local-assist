using System.Text.Json;
using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.HttpClients.Nominatim;
using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.CustomTools.Location.GetCoordinates;

internal sealed class GetCoordinatesCustomToolHandler : ICustomToolHandler<GetCoordinatesCustomTool>
{
    private readonly INominatimHttpClient _nominatinHttpClient;

    public GetCoordinatesCustomToolHandler(INominatimHttpClient nomatinHttpClient)
    {
        _nominatinHttpClient = nomatinHttpClient;
    }

    public async Task<PromptResult> HandleAsync(
        GetCoordinatesCustomTool tool,
        CancellationToken cancellationToken = default)
    {
        var result = await _nominatinHttpClient.GetGeoInfoAsync(
           tool.City,
           tool.State,
           cancellationToken);

        if (result.IsFailure)
        {
            return PromptResult.Failure(result.Error.Description);
        }

        return PromptResult.Success(JsonSerializer.Serialize(result.Value.FirstOrDefault()));
    }
}
