using System.Net;
using Local.Assist.Domain.Shared;

namespace Local.Assist.Domain.Errors;

public static class NomatinHttpClientErrors
{
    public static Error GetGeoInfoEmptyResponse => new(HttpStatusCode.InternalServerError, "GET_GEO_INFO_EMPTY_RESPONSE", "The response content was empty.");
    public static Error GetGeoInfoDeserializeError => new(HttpStatusCode.InternalServerError, "GET_GEO_INFO_DESERIALIZE_ERROR", "The response content could not be deserialized.");
    public static Error GetGeoInfoUnknownError => new(HttpStatusCode.InternalServerError, "GET_GEO_INFO_UNKNOWN_ERROR", "An unknown error occurred while retrieving the geo info.");
}
