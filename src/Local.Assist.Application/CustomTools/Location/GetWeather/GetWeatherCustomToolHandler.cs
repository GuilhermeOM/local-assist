using System.Text.Json;
using Local.Assist.Application.Abstractions.Messaging;
using Local.Assist.Domain.HttpClients.OpenMeteo;
using Local.Assist.Domain.ValueObjects;

namespace Local.Assist.Application.CustomTools.Location.GetWeather;

internal sealed class GetWeatherCustomToolHandler : ICustomToolHandler<GetWeatherCustomTool>
{
    private readonly IOpenMeteoHttpClient _openMeteoHttpClient;

    public GetWeatherCustomToolHandler(IOpenMeteoHttpClient openMeteoHttpClient)
    {
        _openMeteoHttpClient = openMeteoHttpClient;
    }

    public async Task<PromptResult> HandleAsync(
        GetWeatherCustomTool tool,
        CancellationToken cancellationToken = default)
    {
        var result = await _openMeteoHttpClient.GetWeatherForecastAsync(
            tool.Latitude,
            tool.Longitude,
            tool.Date,
            cancellationToken);

        if (result.IsFailure)
        {
            return PromptResult.Failure(result.Error.Description);
        }

        return PromptResult.Success(JsonSerializer.Serialize(result.Value));
    }
}
