using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class ExobiologyStats
{
    [JsonPropertyName("Organic_Data_Profits")]
    public long? ExobiologyProfits { get; set; }

    [JsonPropertyName("First_Logged_Profits")]
    public long? FirstLoggedProfits { get; set; }

    [JsonPropertyName("Organic_Genus_Encountered")]
    public int? OrganicGenusEncountered { get; set; }

    [JsonPropertyName("Organic_Species_Encountered")]
    public int? OrganicSpeciesEncountered { get; set; }

    [JsonPropertyName("Organic_Data")]
    public int? OrganicData { get; set; }

    [JsonPropertyName("Organic_Systems")]
    public int? OrganicSystems { get; set; }

    [JsonPropertyName("Organic_Planets")]
    public int? OrganicPlanets { get; set; }
}
