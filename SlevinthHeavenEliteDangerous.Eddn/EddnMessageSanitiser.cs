using System.Text.Json.Nodes;

namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Strips EDDN-disallowed fields from raw journal JSON and returns a cleaned JsonObject.
/// Also provides augmentation of StarSystem/StarPos/SystemAddress from context.
/// </summary>
public static class EddnMessageSanitiser
{
    private static readonly HashSet<string> FsdJumpStrip = new(StringComparer.Ordinal)
        { "Wanted", "BoostUsed", "FuelLevel", "FuelUsed", "JumpDist" };

    // CarrierJump has the same disallowed fields as FSDJump
    private static readonly HashSet<string> CarrierJumpStrip = FsdJumpStrip;

    private static readonly HashSet<string> FsdJumpFactionStrip = new(StringComparer.Ordinal)
        { "HappiestSystem", "HomeSystem", "MyReputation", "SquadronFaction" };

    private static readonly HashSet<string> LocationStrip = new(StringComparer.Ordinal)
        { "Wanted", "Latitude", "Longitude" };

    private static readonly HashSet<string> DockedStrip = new(StringComparer.Ordinal)
        { "Wanted", "ActiveFine", "CockpitBreach" };

    private static readonly HashSet<string> CodexEntryStrip = new(StringComparer.Ordinal)
        { "IsNewEntry", "NewTraitsDiscovered" };

    /// <summary>
    /// Parse and sanitise a raw journal JSON line for the given event type.
    /// Returns null if the JSON cannot be parsed.
    /// </summary>
    public static JsonObject? Sanitise(string jsonLine, string eventName)
    {
        JsonNode? node;
        try { node = JsonNode.Parse(jsonLine); }
        catch { return null; }

        if (node is not JsonObject obj) return null;

        // Strip all _Localised keys from the top-level object
        StripLocalisedKeys(obj);

        switch (eventName)
        {
            case "FSDJump":
            case "CarrierJump":
                StripKeys(obj, FsdJumpStrip);
                StripFactionKeys(obj, FsdJumpFactionStrip);
                break;
            case "Location":
                StripKeys(obj, LocationStrip);
                StripFactionKeys(obj, FsdJumpFactionStrip);
                break;
            case "Docked":
                StripKeys(obj, DockedStrip);
                break;
            case "Scan":
                StripLocalisedFromArrayItems(obj, "Materials");
                break;
            case "CodexEntry":
                StripKeys(obj, CodexEntryStrip);
                break;
            case "FSSBodySignals":
                StripFieldFromArrayItems(obj, "Signals", "Type_Localised");
                break;
        }

        return obj;
    }

    /// <summary>
    /// Augment a sanitised message with StarSystem, StarPos, SystemAddress from context
    /// where they are not already present.
    /// </summary>
    public static void Augment(JsonObject obj, string? starSystem, double[]? starPos, long? systemAddress)
    {
        if (starSystem != null && !obj.ContainsKey("StarSystem"))
            obj["StarSystem"] = starSystem;

        if (starPos != null && !obj.ContainsKey("StarPos"))
        {
            var arr = new JsonArray();
            foreach (var d in starPos) arr.Add(d);
            obj["StarPos"] = arr;
        }

        if (systemAddress.HasValue && !obj.ContainsKey("SystemAddress"))
            obj["SystemAddress"] = systemAddress.Value;
    }

    private static void StripLocalisedKeys(JsonObject obj)
    {
        var keysToRemove = obj
            .Select(kvp => kvp.Key)
            .Where(k => k.EndsWith("_Localised", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
            obj.Remove(key);
    }

    private static void StripKeys(JsonObject obj, HashSet<string> keys)
    {
        foreach (var key in keys)
            obj.Remove(key);
    }

    private static void StripFactionKeys(JsonObject obj, HashSet<string> keys)
    {
        if (!obj.TryGetPropertyValue("Factions", out var factionsNode) || factionsNode is not JsonArray arr)
            return;

        foreach (var item in arr)
        {
            if (item is not JsonObject faction) continue;
            StripLocalisedKeys(faction);
            StripKeys(faction, keys);
        }
    }

    private static void StripLocalisedFromArrayItems(JsonObject obj, string arrayProp)
    {
        if (!obj.TryGetPropertyValue(arrayProp, out var arrNode) || arrNode is not JsonArray arr)
            return;

        foreach (var item in arr)
        {
            if (item is JsonObject itemObj)
                StripLocalisedKeys(itemObj);
        }
    }

    private static void StripFieldFromArrayItems(JsonObject obj, string arrayProp, string fieldToStrip)
    {
        if (!obj.TryGetPropertyValue(arrayProp, out var arrNode) || arrNode is not JsonArray arr)
            return;

        foreach (var item in arr)
        {
            if (item is JsonObject itemObj)
                itemObj.Remove(fieldToStrip);
        }
    }
}
