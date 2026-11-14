using System.Text.Json.Serialization;
using Local.Assist.Domain.Extensions;

namespace Local.Assist.Domain.HttpClients.Nominatim.Responses;

public sealed record GetGeoInfoResponse(
    [property: JsonPropertyName("lat"), JsonConverter(typeof(JsonConverters.StringToDoubleConverter))] double Lat,
    [property: JsonPropertyName("lon"), JsonConverter(typeof(JsonConverters.StringToDoubleConverter))] double Lon,
    [property: JsonPropertyName("display_name")] string DisplayName);
