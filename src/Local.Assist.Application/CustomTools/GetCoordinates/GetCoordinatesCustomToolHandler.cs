using System.Text.Json;
using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.HttpClients.Nomatin;
using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.CustomTools.GetCoordinates;

internal sealed class GetCoordinatesCustomToolHandler : ICustomToolHandler<GetCoordinatesCustomTool>
{
    private readonly INomatinHttpClient _nomatinHttpClient;

    public GetCoordinatesCustomToolHandler(INomatinHttpClient nomatinHttpClient)
    {
        _nomatinHttpClient = nomatinHttpClient;
    }

    public async Task<PromptResult> HandleAsync(
        GetCoordinatesCustomTool tool,
        CancellationToken cancellationToken = default)
    {
        var result = await _nomatinHttpClient.GetGeoInfoAsync(
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
