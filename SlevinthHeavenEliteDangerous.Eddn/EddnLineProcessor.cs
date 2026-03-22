using System.Text.Json;
using System.Text.Json.Nodes;

namespace SlevinthHeavenEliteDangerous.Eddn;

public sealed class EddnLineResult
{
    /// <summary>Events ready to enqueue for sending (immediate + released pending events).</summary>
    public List<EddnPendingEvent> EventsToSend { get; } = [];

    /// <summary>True if state was mutated and should be persisted.</summary>
    public bool StateMutated { get; set; }
}

/// <summary>
/// Stateless per-line EDDN processor. Mutates the provided <see cref="EddnCommanderState"/>
/// and returns an <see cref="EddnLineResult"/> describing what needs to be sent.
/// </summary>
public static class EddnLineProcessor
{
    private const string JournalSchema = "https://eddn.edcd.io/schemas/journal/1";
    private const string FssBodySignalsSchema = "https://eddn.edcd.io/schemas/fssbodysignals/1";
    private const string CodexEntrySchema = "https://eddn.edcd.io/schemas/codexentry/1";
    private const string ApproachSettlementSchema = "https://eddn.edcd.io/schemas/approachsettlement/1";
    private const string ScanBaryCentreSchema = "https://eddn.edcd.io/schemas/scanbarycentre/1";
    private const string NavBeaconSchema = "https://eddn.edcd.io/schemas/navbeacon/1";

    public static EddnLineResult ProcessLine(string jsonLine, EddnCommanderState state)
    {
        var result = new EddnLineResult();

        JsonDocument doc;
        try { doc = JsonDocument.Parse(jsonLine); }
        catch { return result; }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("event", out var eventProp))
                return result;

            var eventName = eventProp.GetString();
            if (string.IsNullOrEmpty(eventName))
                return result;

            switch (eventName)
            {
                case "Fileheader":
                    ProcessFileheader(root, state, result);
                    break;
                case "LoadGame":
                    ProcessLoadGame(root, state, result);
                    break;
                case "FSDJump":
                    ProcessFsdJump(root, jsonLine, state, result);
                    break;
                case "CarrierJump":
                    ProcessCarrierJump(root, jsonLine, state, result);
                    break;
                case "Location":
                    ProcessLocation(root, jsonLine, state, result);
                    break;
                case "Docked":
                    ProcessDocked(root, jsonLine, state, result);
                    break;
                case "Scan":
                    ProcessScan(root, jsonLine, state, result);
                    break;
                case "SAASignalsFound":
                    ProcessSaaSignalsFound(root, jsonLine, state, result);
                    break;
                case "FSSBodySignals":
                    ProcessFssBodySignals(root, jsonLine, state, result);
                    break;
                case "FSSAllBodiesFound":
                    ProcessFssAllBodiesFound(root, jsonLine, state, result);
                    break;
                case "FSSSignalDiscovered":
                    ProcessFssSignalDiscovered(root, jsonLine, state, result);
                    break;
                case "ScanBaryCentre":
                    ProcessScanBaryCentre(root, jsonLine, state, result);
                    break;
                case "ApproachSettlement":
                    ProcessApproachSettlement(root, jsonLine, state, result);
                    break;
                case "NavBeaconScan":
                    ProcessNavBeaconScan(root, jsonLine, state, result);
                    break;
                case "CodexEntry":
                    ProcessCodexEntry(root, jsonLine, state, result);
                    break;
                case "SellExplorationData":
                    ProcessSellExplorationData(root, state, result);
                    break;
                case "MultiSellExplorationData":
                    ProcessMultiSellExplorationData(root, state, result);
                    break;
                case "SellOrganicData":
                    ProcessSellOrganicData(state, result);
                    break;
            }
        }

        return result;
    }

    // ----- event handlers -----

    private static void ProcessFileheader(JsonElement root, EddnCommanderState state, EddnLineResult result)
    {
        var version = GetString(root, "gameversion");
        var build = GetString(root, "build");

        if (!string.IsNullOrEmpty(version)) state.GameInfo.GameVersion = version;
        if (!string.IsNullOrEmpty(build)) state.GameInfo.GameBuild = build;
        result.StateMutated = true;
    }

    private static void ProcessLoadGame(JsonElement root, EddnCommanderState state, EddnLineResult result)
    {
        var version = GetString(root, "gameversion");
        var build = GetString(root, "build");
        var commander = GetString(root, "Commander");

        if (!string.IsNullOrEmpty(version)) state.GameInfo.GameVersion = version;
        if (!string.IsNullOrEmpty(build)) state.GameInfo.GameBuild = build;
        if (!string.IsNullOrEmpty(commander)) state.GameInfo.CommanderName = commander;

        if (root.TryGetProperty("Horizons", out var h) && (h.ValueKind == JsonValueKind.True || h.ValueKind == JsonValueKind.False))
            state.GameInfo.Horizons = h.GetBoolean();
        if (root.TryGetProperty("Odyssey", out var o) && (o.ValueKind == JsonValueKind.True || o.ValueKind == JsonValueKind.False))
            state.GameInfo.Odyssey = o.GetBoolean();

        result.StateMutated = true;
    }

    private static void ProcessFsdJump(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        // Update current system context
        state.CurrentSystem = GetString(root, "StarSystem");
        state.CurrentSystemAddress = sysAddr;
        state.CurrentStarPos = GetDoubleArray(root, "StarPos");
        // Persist star position for this system when available so pending events
        // can be augmented later if they lack StarPos.
        if (state.CurrentStarPos != null && state.CurrentSystemAddress.HasValue)
            state.SystemPositions[state.CurrentSystemAddress.Value] = state.CurrentStarPos;
        result.StateMutated = true;

        var key = $"fsdjump_{sysAddr}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "FSDJump");
        if (obj == null) return;

        // Hold until SellExplorationData — broadcasting FSDJump immediately would let anyone
        // watching EDDN track visited systems and race to claim first discoveries.
        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
    }

    private static void ProcessLocation(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        // Update current system context
        state.CurrentSystem = GetString(root, "StarSystem");
        state.CurrentSystemAddress = sysAddr;
        state.CurrentStarPos = GetDoubleArray(root, "StarPos");
        if (state.CurrentStarPos != null && state.CurrentSystemAddress.HasValue)
            state.SystemPositions[state.CurrentSystemAddress.Value] = state.CurrentStarPos;
        result.StateMutated = true;

        var key = $"location_{sysAddr}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "Location");
        if (obj == null) return;

        // Hold until SellExplorationData — same reasoning as FSDJump.
        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
    }

    private static void ProcessScan(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var bodyId = GetNullableInt(root, "BodyID");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || bodyId == null || timestamp == null) return;

        // Cross-check: if we know the current system, verify address matches
        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;

        // Skip if we have no system context — can't augment or match for release
        if (state.CurrentSystem == null) return;

        var key = $"scan_{sysAddr}_{bodyId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "Scan");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessSaaSignalsFound(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var bodyId = GetNullableInt(root, "BodyID");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || bodyId == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var key = $"saa_{sysAddr}_{bodyId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "SAASignalsFound");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessFssBodySignals(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var bodyId = GetNullableInt(root, "BodyID");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || bodyId == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var key = $"fssbody_{sysAddr}_{bodyId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "FSSBodySignals");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = FssBodySignalsSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessCarrierJump(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        // Update current system context (same as FSDJump)
        state.CurrentSystem = GetString(root, "StarSystem");
        state.CurrentSystemAddress = sysAddr;
        state.CurrentStarPos = GetDoubleArray(root, "StarPos");
        if (state.CurrentStarPos != null && state.CurrentSystemAddress.HasValue)
            state.SystemPositions[state.CurrentSystemAddress.Value] = state.CurrentStarPos;
        result.StateMutated = true;

        var key = $"carrierjump_{sysAddr}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "CarrierJump");
        if (obj == null) return;

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
    }

    private static void ProcessDocked(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var marketId = GetNullableLong(root, "MarketID");
        var timestamp = GetString(root, "timestamp");
        if (marketId == null || timestamp == null) return;

        var key = $"docked_{marketId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "Docked");
        if (obj == null) return;

        // Docked events don't include StarSystem — augment from current context
        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, state.CurrentSystemAddress);

        // Only send if we have system context (needed for journal/1 StarSystem requirement)
        if (!obj.ContainsKey("StarSystem")) return;

        result.EventsToSend.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = state.CurrentSystemAddress ?? 0,
        });
    }

    private static void ProcessFssAllBodiesFound(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var key = $"allbodies_{sysAddr}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "FSSAllBodiesFound");
        if (obj == null) return;

        // FSSAllBodiesFound uses SystemName — map it to StarSystem for journal/1
        if (!obj.ContainsKey("StarSystem") && obj.TryGetPropertyValue("SystemName", out var sn))
            obj["StarSystem"] = sn?.DeepClone();

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessFssSignalDiscovered(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        // Build a stable key from system + timestamp + signal name
        var signalName = GetString(root, "SignalName") ?? string.Empty;
        var safeSignal = string.Concat(signalName.Where(char.IsLetterOrDigit)).ToLowerInvariant();
        var key = $"signal_{sysAddr}_{timestamp}_{safeSignal}";

        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "FSSSignalDiscovered");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = JournalSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessScanBaryCentre(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var bodyId = GetNullableInt(root, "BodyID");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || bodyId == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var key = $"barycentre_{sysAddr}_{bodyId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "ScanBaryCentre");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = ScanBaryCentreSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessApproachSettlement(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var bodyId = GetNullableInt(root, "BodyID");
        var key = $"settlement_{sysAddr}_{bodyId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "ApproachSettlement");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = ApproachSettlementSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessNavBeaconScan(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        var sysAddr = GetNullableLong(root, "SystemAddress");
        var timestamp = GetString(root, "timestamp");
        if (sysAddr == null || timestamp == null) return;

        if (state.CurrentSystemAddress.HasValue && sysAddr != state.CurrentSystemAddress) return;
        if (state.CurrentSystem == null) return;

        var key = $"navbeacon_{sysAddr}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingNavEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "NavBeaconScan");
        if (obj == null) return;

        EddnMessageSanitiser.Augment(obj, state.CurrentSystem, state.CurrentStarPos, null);

        state.PendingNavEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = NavBeaconSchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = sysAddr.Value,
        });
        result.StateMutated = true;
    }

    private static void ProcessCodexEntry(
        JsonElement root, string jsonLine, EddnCommanderState state, EddnLineResult result)
    {
        // Only send new codex entries
        if (!GetBool(root, "IsNewEntry")) return;

        var entryId = GetNullableLong(root, "EntryID");
        var timestamp = GetString(root, "timestamp");
        if (entryId == null || timestamp == null) return;

        var key = $"codex_{entryId}_{timestamp}";
        if (state.SentEventKeys.Contains(key)) return;
        if (state.PendingCodexEvents.Any(e => e.EventKey == key)) return;

        var obj = EddnMessageSanitiser.Sanitise(jsonLine, "CodexEntry");
        if (obj == null) return;

        // Omit BodyName/BodyID — we don't track Status.json or ApproachBody server-side
        obj.Remove("BodyName");
        obj.Remove("BodyID");

        state.PendingCodexEvents.Add(new EddnPendingEvent
        {
            EventKey = key,
            SchemaRef = CodexEntrySchema,
            MessageJson = obj.ToJsonString(),
            SystemAddress = 0,
        });
        result.StateMutated = true;
    }

    private static void ProcessSellExplorationData(
        JsonElement root, EddnCommanderState state, EddnLineResult result)
    {
        var soldSystems = new HashSet<string>(StringComparer.Ordinal);

        if (root.TryGetProperty("Systems", out var systems) && systems.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in systems.EnumerateArray())
            {
                var name = s.GetString();
                if (!string.IsNullOrEmpty(name)) soldSystems.Add(name);
            }
        }

        ReleaseNavEventsForSystems(soldSystems, state, result);
    }

    private static void ProcessMultiSellExplorationData(
        JsonElement root, EddnCommanderState state, EddnLineResult result)
    {
        var soldSystems = new HashSet<string>(StringComparer.Ordinal);

        if (root.TryGetProperty("Discovered", out var discovered) && discovered.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in discovered.EnumerateArray())
            {
                if (entry.TryGetProperty("SystemName", out var sn) && sn.ValueKind == JsonValueKind.String)
                {
                    var name = sn.GetString();
                    if (!string.IsNullOrEmpty(name)) soldSystems.Add(name);
                }
            }
        }

        ReleaseNavEventsForSystems(soldSystems, state, result);
    }

    private static void ProcessSellOrganicData(EddnCommanderState state, EddnLineResult result)
    {
        if (state.PendingCodexEvents.Count == 0) return;

        result.EventsToSend.AddRange(state.PendingCodexEvents);
        state.PendingCodexEvents.Clear();
        result.StateMutated = true;
    }

    private static void ReleaseNavEventsForSystems(
        HashSet<string> soldSystemNames, EddnCommanderState state, EddnLineResult result)
    {
        if (soldSystemNames.Count == 0 || state.PendingNavEvents.Count == 0) return;

        var toKeep = new List<EddnPendingEvent>(state.PendingNavEvents.Count);

        foreach (var pending in state.PendingNavEvents)
        {
            string? systemName = null;
            try
            {
                var msg = JsonNode.Parse(pending.MessageJson);
                systemName = msg?["StarSystem"]?.GetValue<string>();
            }
            catch { }

            if (!string.IsNullOrEmpty(systemName) && soldSystemNames.Contains(systemName))
                result.EventsToSend.Add(pending);
            else
                toKeep.Add(pending);
        }

        if (result.EventsToSend.Count > 0)
        {
            state.PendingNavEvents.Clear();
            state.PendingNavEvents.AddRange(toKeep);
            result.StateMutated = true;
        }
    }

    // ----- helpers -----

    private static string? GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int? GetNullableInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt32(out var i) ? i : null;

    private static long? GetNullableLong(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt64(out var l) ? l : null;

    private static bool GetBool(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;

    private static double[]? GetDoubleArray(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind != JsonValueKind.Array)
            return null;

        var list = new List<double>();
        foreach (var item in v.EnumerateArray())
        {
            if (item.TryGetDouble(out var d))
                list.Add(d);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }
}
