using Local.Assist.Application.Abstractions.Messaging;

namespace Local.Assist.Application.CustomTools.Location.GetCoordinates;

public sealed record GetCoordinatesCustomTool(string City, string State) : ICustomTool;
