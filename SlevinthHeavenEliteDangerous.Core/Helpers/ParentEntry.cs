using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Helpers;

[JsonConverter(typeof(ParentEntryJsonConverter))]
public class ParentEntry
{
    // e.g. { "Star": 6 } -> Type = "Star", Id = 6
    public string Type { get; set; } = string.Empty;

    public long? Id { get; set; }
}

internal class ParentEntryJsonConverter : JsonConverter<ParentEntry>
{
    public override ParentEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using var doc = JsonDocument.ParseValue(ref reader);
        var obj = doc.RootElement;
        foreach (var prop in obj.EnumerateObject())
        {
            var name = prop.Name;
            if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt64(out var id))
            {
                return new ParentEntry { Type = name, Id = id };
            }
            if (prop.Value.ValueKind == JsonValueKind.String)
            {
                // try parse string as long
                if (long.TryParse(prop.Value.GetString(), out var parsed))
                    return new ParentEntry { Type = name, Id = parsed };
            }
        }

        return new ParentEntry();
    }

    public override void Write(Utf8JsonWriter writer, ParentEntry value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.Id.HasValue)
            writer.WriteNumber(value.Type ?? string.Empty, value.Id.Value);
        else
            writer.WriteString(value.Type ?? string.Empty, string.Empty);
        writer.WriteEndObject();
    }
}
