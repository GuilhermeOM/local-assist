using Local.Assist.Application.CustomTools.Location.GetCoordinates;
using Local.Assist.Application.CustomTools.Location.GetWeather;
using Local.Assist.Application.CustomTools.System.GetCurrentDateTime;
using OllamaSharp;
using System.Globalization;

namespace Local.Assist.Application.CustomTools;

public sealed class Tools
{
    #region System Tools
    /// <summary>
    /// Obtem a data e hora atual do sistema.
    /// </summary>
    [OllamaTool]
    public static GetCurrentDateTimeCustomTool GetCurrentDateTime() => new();
    #endregion

    #region Location Tools
     /// <summary>
    /// Obtem as coordenadas geográficas (latitude e longitude) de uma cidade e estado específicos.
    /// </summary>
    /// <param name="city">Nome da cidade.</param>
    /// <param name="state">Nome do estado da cidade.</param>
    [OllamaTool]
    public static GetCoordinatesCustomTool GetCoordinates(string city, string state) => new(city, state);

    /// <summary>
    /// Obtem a previsão do tempo para uma determinada latitude, longitude e data.
    /// </summary>
    /// <param name="latitude">Latitude do local.</param>
    /// <param name="longitude">Longitude do local.</param>
    /// <param name="date">Data para a qual se deseja obter a previsão do tempo (yyyy-MM-dd).</param>
    [OllamaTool]
    public static GetWeatherCustomTool GetWeather(double latitude, double longitude, string date)
    {
        if (!DateOnly.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            throw new ArgumentException("Not a valid date", nameof(date));
        }

        return new(latitude, longitude, parsedDate);
    }
    #endregion
}
