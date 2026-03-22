using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Helpers;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class ScanEvent : EventBase
{
    [JsonPropertyName("ScanType")]
    public string ScanType { get; set; } = string.Empty;

    [JsonIgnore]
    public ScanTypeEnum? ScanTypeParsed
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ScanType))
                return null;
            if (System.Enum.TryParse<ScanTypeEnum>(ScanType, out var v))
                return v;
            return ScanTypeEnum.Unknown;
        }
    }

    [JsonPropertyName("BodyName")]
    public string BodyName { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("DistanceFromArrivalLS")]
    public double? DistanceFromArrivalLS { get; set; }

    [JsonPropertyName("StarType")]
    public string StarType { get; set; } = string.Empty;

    [JsonPropertyName("WasDiscovered")]
    public bool? WasDiscovered { get; set; }

    [JsonPropertyName("WasMapped")]
    public bool? WasMapped { get; set; }

    [JsonPropertyName("WasFootfalled")]
    public bool? WasFootfalled { get; set; }

    [JsonPropertyName("Landable")]
    public bool? Landable { get; set; }

    [JsonPropertyName("TerraformState")]
    public string TerraformState { get; set; } = string.Empty;

    [JsonPropertyName("PlanetClass")]
    public string PlanetClass { get; set; } = string.Empty;

    [JsonPropertyName("Atmosphere")]
    public string Atmosphere { get; set; } = string.Empty;

    [JsonPropertyName("AtmosphereType")]
    public string AtmosphereType { get; set; } = string.Empty;

    [JsonPropertyName("AtmosphereComposition")]
    public List<AtmosphereComponent> AtmosphereComposition { get; set; } = [];

    [JsonPropertyName("Volcanism")]
    public string Volcanism { get; set; } = string.Empty;

    [JsonPropertyName("SurfaceTemperature")]
    public double? SurfaceTemperature { get; set; }

    [JsonPropertyName("SurfacePressure")]
    public double? SurfacePressure { get; set; }

    [JsonPropertyName("SurfaceGravity")]
    public double? SurfaceGravity { get; set; }

    [JsonPropertyName("TidalLock")]
    public bool? TidalLock { get; set; }

    [JsonPropertyName("MassEM")]
    public double? MassEM { get; set; }

    [JsonPropertyName("Radius")]
    public double? Radius { get; set; }

    [JsonPropertyName("SemiMajorAxis")]
    public double? SemiMajorAxis { get; set; }

    [JsonPropertyName("Eccentricity")]
    public double? Eccentricity { get; set; }

    [JsonPropertyName("OrbitalInclination")]
    public double? OrbitalInclination { get; set; }

    [JsonPropertyName("Periapsis")]
    public double? Periapsis { get; set; }

    [JsonPropertyName("OrbitalPeriod")]
    public double? OrbitalPeriod { get; set; }

    [JsonPropertyName("AscendingNode")]
    public double? AscendingNode { get; set; }

    [JsonPropertyName("MeanAnomaly")]
    public double? MeanAnomaly { get; set; }

    [JsonPropertyName("RotationPeriod")]
    public double? RotationPeriod { get; set; }

    [JsonPropertyName("AxialTilt")]
    public double? AxialTilt { get; set; }

    [JsonPropertyName("StellarMass")]
    public double? StellarMass { get; set; }

    [JsonPropertyName("AbsoluteMagnitude")]
    public double? AbsoluteMagnitude { get; set; }

    [JsonPropertyName("Age_MY")]
    public int? Age_MY { get; set; }

    [JsonPropertyName("Luminosity")]
    public string Luminosity { get; set; } = string.Empty;

    [JsonPropertyName("Subclass")]
    public int? Subclass { get; set; }

    [JsonPropertyName("ReserveLevel")]
    public string ReserveLevel { get; set; } = string.Empty;

    [JsonPropertyName("Composition")]
    public Dictionary<string, double> Composition { get; set; } = [];

    [JsonPropertyName("Parents")]
    public List<ParentEntry> Parents { get; set; } = [];

    [JsonPropertyName("Rings")]
    public List<RingInfo> Rings { get; set; } = [];

    [JsonPropertyName("Materials")]
    public List<MaterialPercent> Materials { get; set; } = [];
}

public class AtmosphereComponent
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Percent")]
    public double Percent { get; set; }
}
