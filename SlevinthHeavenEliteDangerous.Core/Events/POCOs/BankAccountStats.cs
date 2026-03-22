using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class BankAccountStats
{
    [JsonPropertyName("Current_Wealth")]
    public long? CurrentWealth { get; set; }

    [JsonPropertyName("Spent_On_Ships")]
    public long? SpentOnShips { get; set; }

    [JsonPropertyName("Spent_On_Outfitting")]
    public long? SpentOnOutfitting { get; set; }

    [JsonPropertyName("Spent_On_Repairs")]
    public long? SpentOnRepairs { get; set; }

    [JsonPropertyName("Spent_On_Fuel")]
    public long? SpentOnFuel { get; set; }

    [JsonPropertyName("Spent_On_Ammo_Consumables")]
    public long? SpentOnAmmoConsumables { get; set; }

    [JsonPropertyName("Insurance_Claims")]
    public int? InsuranceClaims { get; set; }

    [JsonPropertyName("Spent_On_Insurance")]
    public long? SpentOnInsurance { get; set; }

    [JsonPropertyName("Owned_Ship_Count")]
    public int? OwnedShipCount { get; set; }

    [JsonPropertyName("Spent_On_Suits")]
    public long? SpentOnSuits { get; set; }

    [JsonPropertyName("Spent_On_Weapons")]
    public long? SpentOnWeapons { get; set; }

    [JsonPropertyName("Spent_On_Suit_Consumables")]
    public long? SpentOnSuitConsumables { get; set; }

    [JsonPropertyName("Suits_Owned")]
    public int? SuitsOwned { get; set; }

    [JsonPropertyName("Weapons_Owned")]
    public int? WeaponsOwned { get; set; }

    [JsonPropertyName("Spent_On_Premium_Stock")]
    public long? SpentOnPremiumStock { get; set; }

    [JsonPropertyName("Premium_Stock_Bought")]
    public int? PremiumStockBought { get; set; }
}
