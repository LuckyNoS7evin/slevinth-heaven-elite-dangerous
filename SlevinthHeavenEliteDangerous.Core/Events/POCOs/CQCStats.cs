using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CQCStats
{
    [JsonPropertyName("CQC_Credits_Earned")]
    public long? CreditsEarned { get; set; }

    [JsonPropertyName("CQC_Time_Played")]
    public long? TimePlayed { get; set; }

    [JsonPropertyName("CQC_KD")]
    public double? KD { get; set; }

    [JsonPropertyName("CQC_Kills")]
    public int? Kills { get; set; }

    [JsonPropertyName("CQC_WL")]
    public int? WinLoss { get; set; }
}
