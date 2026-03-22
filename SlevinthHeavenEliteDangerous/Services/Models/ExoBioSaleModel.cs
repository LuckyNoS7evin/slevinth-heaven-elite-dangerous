using System;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Pure data model representing a single exobiology sale transaction
/// </summary>
public class ExoBioSaleModel
{
    public DateTime SaleTimestamp { get; set; }
    public long MarketID { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public List<ExoBioSaleItem> ItemsSold { get; set; } = [];
    public long TotalValue { get; set; }
    public long TotalBonus { get; set; }
}

/// <summary>
/// Pure data model for an individual item within a sale
/// </summary>
public class ExoBioSaleItem
{
    public string Species_Localised { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public long Value { get; set; }
    public long Bonus { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public double DistanceFromSol { get; set; }
    public double[]? StarPos { get; set; }
}
