using System.Text.Json;
using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.HttpClients.Nominatin;
using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.CustomTools.GetCoordinates;

internal sealed class GetCoordinatesCustomToolHandler : ICustomToolHandler<GetCoordinatesCustomTool>
{
    private readonly INominatinHttpClient _nominatinHttpClient;

    public GetCoordinatesCustomToolHandler(INominatinHttpClient nomatinHttpClient)
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
