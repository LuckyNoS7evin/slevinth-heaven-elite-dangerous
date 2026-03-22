using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class StatisticsEvent : EventBase
{
    [JsonPropertyName("Bank_Account")]
    public BankAccountStats? BankAccount { get; set; }

    [JsonPropertyName("Combat")]
    public CombatStats? Combat { get; set; }

    [JsonPropertyName("Crime")]
    public CrimeStats? Crime { get; set; }

    [JsonPropertyName("Smuggling")]
    public SmugglingStats? Smuggling { get; set; }

    [JsonPropertyName("Trading")]
    public TradingStats? Trading { get; set; }

    [JsonPropertyName("Mining")]
    public MiningStats? Mining { get; set; }

    [JsonPropertyName("Exploration")]
    public ExplorationStats? Exploration { get; set; }

    [JsonPropertyName("Passengers")]
    public PassengerStats? Passengers { get; set; }

    [JsonPropertyName("Crew")]
    public CrewStats? Crew { get; set; }

    [JsonPropertyName("CQC")]
    public CQCStats? CQC { get; set; }

    [JsonPropertyName("Crafting")]
    public object? Crafting { get; set; }

    [JsonPropertyName("Exobiology")]
    public ExobiologyStats? Exobiology { get; set; }

    [JsonPropertyName("Material_Trader_Stats")]
    public object? Material_Trader_Stats { get; set; }

    [JsonPropertyName("Multicrew")]
    public object? Multicrew { get; set; }

    [JsonPropertyName("Search_And_Rescue")]
    public object? Search_And_Rescue { get; set; }

    [JsonPropertyName("Squadron")]
    public object? Squadron { get; set; }
}
