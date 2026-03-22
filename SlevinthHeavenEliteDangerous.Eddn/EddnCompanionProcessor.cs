using System.Text.Json;
using System.Text.Json.Nodes;

namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Transforms ED companion JSON files (Market.json, Shipyard.json, Outfitting.json,
/// FCMaterials.json) into EDDN message payloads.
/// </summary>
public static class EddnCompanionProcessor
{
    private const string CommoditySchema = "https://eddn.edcd.io/schemas/commodity/3";
    private const string ShipyardSchema = "https://eddn.edcd.io/schemas/shipyard/2";
    private const string OutfittingSchema = "https://eddn.edcd.io/schemas/outfitting/2";
    private const string FcMaterialsSchema = "https://eddn.edcd.io/schemas/fcmaterials/1";

    /// <summary>
    /// Process a companion file. <paramref name="type"/> is one of:
    /// market, shipyard, outfitting, fcmaterials.
    /// Returns null if the file cannot be parsed or is missing required fields.
    /// </summary>
    public static EddnPendingEvent? Process(string type, string json, EddnGameInfo gameInfo) =>
        type.ToLowerInvariant() switch
        {
            "market" => ProcessMarket(json, gameInfo),
            "shipyard" => ProcessShipyard(json, gameInfo),
            "outfitting" => ProcessOutfitting(json, gameInfo),
            "fcmaterials" => ProcessFcMaterials(json),
            _ => null,
        };

    // ----- Market.json → commodity/3 -----

    private static EddnPendingEvent? ProcessMarket(string json, EddnGameInfo gameInfo)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch { return null; }

        using (doc)
        {
            var root = doc.RootElement;

            var systemName = GetString(root, "StarSystem");
            var stationName = GetString(root, "StationName");
            var timestamp = GetString(root, "timestamp");
            if (systemName == null || stationName == null || timestamp == null) return null;

            var marketId = GetLong(root, "MarketID");

            var message = new JsonObject
            {
                ["systemName"] = systemName,
                ["stationName"] = stationName,
                ["marketId"] = marketId,
                ["timestamp"] = timestamp,
            };

            if (gameInfo.Horizons.HasValue) message["horizons"] = gameInfo.Horizons.Value;
            if (gameInfo.Odyssey.HasValue) message["odyssey"] = gameInfo.Odyssey.Value;

            var commodities = new JsonArray();
            if (root.TryGetProperty("Items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    var rawName = GetString(item, "Name");
                    if (string.IsNullOrEmpty(rawName)) continue;

                    var isRare = GetBool(item, "Rare");
                    var statusFlags = isRare ? new JsonArray { "rare" } : new JsonArray();

                    commodities.Add(new JsonObject
                    {
                        ["name"] = StripEdName(rawName),
                        ["meanPrice"] = GetInt(item, "MeanPrice"),
                        ["buyPrice"] = GetInt(item, "BuyPrice"),
                        ["stock"] = GetInt(item, "Stock"),
                        ["stockBracket"] = GetInt(item, "StockBracket"),
                        ["sellPrice"] = GetInt(item, "SellPrice"),
                        ["demand"] = GetInt(item, "Demand"),
                        ["demandBracket"] = GetInt(item, "DemandBracket"),
                        ["statusFlags"] = statusFlags,
                    });
                }
            }

            message["commodities"] = commodities;

            return CompanionEvent(CommoditySchema, message.ToJsonString());
        }
    }

    // ----- Shipyard.json → shipyard/2 -----

    private static EddnPendingEvent? ProcessShipyard(string json, EddnGameInfo gameInfo)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch { return null; }

        using (doc)
        {
            var root = doc.RootElement;

            var systemName = GetString(root, "StarSystem");
            var stationName = GetString(root, "StationName");
            var timestamp = GetString(root, "timestamp");
            if (systemName == null || stationName == null || timestamp == null) return null;

            var message = new JsonObject
            {
                ["systemName"] = systemName,
                ["stationName"] = stationName,
                ["marketId"] = GetLong(root, "MarketID"),
                ["timestamp"] = timestamp,
            };

            if (gameInfo.Horizons.HasValue) message["horizons"] = gameInfo.Horizons.Value;
            if (gameInfo.Odyssey.HasValue) message["odyssey"] = gameInfo.Odyssey.Value;

            var ships = new JsonArray();
            if (root.TryGetProperty("Ships", out var shipArr) && shipArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var ship in shipArr.EnumerateArray())
                {
                    var shipType = GetString(ship, "ShipType");
                    if (!string.IsNullOrEmpty(shipType))
                        ships.Add(shipType);
                }
            }

            message["ships"] = ships;

            return CompanionEvent(ShipyardSchema, message.ToJsonString());
        }
    }

    // ----- Outfitting.json → outfitting/2 -----

    private static EddnPendingEvent? ProcessOutfitting(string json, EddnGameInfo gameInfo)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch { return null; }

        using (doc)
        {
            var root = doc.RootElement;

            var systemName = GetString(root, "StarSystem");
            var stationName = GetString(root, "StationName");
            var timestamp = GetString(root, "timestamp");
            if (systemName == null || stationName == null || timestamp == null) return null;

            var message = new JsonObject
            {
                ["systemName"] = systemName,
                ["stationName"] = stationName,
                ["marketId"] = GetLong(root, "MarketID"),
                ["timestamp"] = timestamp,
            };

            if (gameInfo.Horizons.HasValue) message["horizons"] = gameInfo.Horizons.Value;
            if (gameInfo.Odyssey.HasValue) message["odyssey"] = gameInfo.Odyssey.Value;

            var modules = new JsonArray();
            if (root.TryGetProperty("Items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    var rawName = GetString(item, "Name");
                    if (string.IsNullOrEmpty(rawName)) continue;

                    if (!item.TryGetProperty("id", out var idProp) || !idProp.TryGetInt64(out var id)) continue;

                    modules.Add(new JsonObject
                    {
                        ["name"] = StripEdName(rawName),
                        ["id"] = id,
                    });
                }
            }

            message["modules"] = modules;

            return CompanionEvent(OutfittingSchema, message.ToJsonString());
        }
    }

    // ----- FCMaterials.json → fcmaterials/1 -----

    private static EddnPendingEvent? ProcessFcMaterials(string json)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch { return null; }

        using (doc)
        {
            var root = doc.RootElement;

            var timestamp = GetString(root, "timestamp");
            var carrierName = GetString(root, "CarrierName");
            var carrierId = GetString(root, "CarrierID");
            if (timestamp == null || carrierName == null || carrierId == null) return null;

            var message = new JsonObject
            {
                ["timestamp"] = timestamp,
                ["MarketID"] = GetLong(root, "MarketID"),
                ["CarrierName"] = carrierName,
                ["CarrierID"] = carrierId,
            };

            // Rebuild Items array with stripped names and no _Localised fields
            if (root.TryGetProperty("Items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                var arr = new JsonArray();
                foreach (var item in items.EnumerateArray())
                {
                    var rawName = GetString(item, "Name");
                    if (string.IsNullOrEmpty(rawName)) continue;

                    if (!item.TryGetProperty("id", out var idProp) || !idProp.TryGetInt64(out var id)) continue;

                    arr.Add(new JsonObject
                    {
                        ["id"] = id,
                        ["Name"] = StripEdName(rawName),
                        ["Price"] = GetInt(item, "Price"),
                        ["Stock"] = GetInt(item, "Stock"),
                        ["Demand"] = GetInt(item, "Demand"),
                    });
                }
                message["Items"] = arr;
            }

            return CompanionEvent(FcMaterialsSchema, message.ToJsonString());
        }
    }

    // ----- helpers -----

    private static EddnPendingEvent CompanionEvent(string schema, string messageJson) =>
        new()
        {
            // Companion files are always sent fresh — no dedup key
            EventKey = string.Empty,
            SchemaRef = schema,
            MessageJson = messageJson,
            SystemAddress = 0,
        };

    /// <summary>
    /// Transform an ED localisation key like "$advanacedcatalysers_name;" into "advanacedcatalysers".
    /// Leaves names that don't match the pattern unchanged.
    /// </summary>
    private static string StripEdName(string name)
    {
        if (name.StartsWith('$')) name = name[1..];
        if (name.EndsWith("_name;", StringComparison.OrdinalIgnoreCase)) name = name[..^6];
        return name;
    }

    private static string? GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int GetInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt32(out var i) ? i : 0;

    private static long GetLong(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt64(out var l) ? l : 0;

    private static bool GetBool(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;
}
