using Local.Assist.Application.Abstractions.Messaging;

namespace Local.Assist.Application.CustomTools.GetWeather;

public sealed record GetWeatherCustomTool(
    double Latitude,
    double Longitude,
    DateOnly Date) : ICustomTool;
