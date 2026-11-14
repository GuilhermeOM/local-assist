using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Local.Assist.Domain.Extensions;

public static class JsonConverters
{
    public sealed class StringToDoubleConverter : JsonConverter<double>
    {
        public override double Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out var numericValue))
            {
                return numericValue;
            }

            if (reader.TokenType == JsonTokenType.String
                && double.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var stringValue))
            {
                return stringValue;
            }

            throw new JsonException("Expected a JSON number or a numeric string.");
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
