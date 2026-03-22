using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class EngineerModifier
{
    [JsonPropertyName("Label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public double? Value { get; set; }

    [JsonPropertyName("OriginalValue")]
    public double? OriginalValue { get; set; }

    [JsonPropertyName("LessIsGood")]
    public bool? LessIsGood { get; set; }

    [JsonPropertyName("ValueStr")]
    public string ValueStr { get; set; } = string.Empty;
}
