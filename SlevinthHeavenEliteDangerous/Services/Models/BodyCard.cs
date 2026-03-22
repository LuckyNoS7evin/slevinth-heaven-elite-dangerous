using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Represents a scanned body within a system
/// </summary>
public class BodyCard
{
    public string BodyName { get; set; } = string.Empty;
    public int BodyID { get; set; }
    public int? PlanetParentID { get; set; }
    public int? StarParentID { get; set; }
    public bool WasDiscovered { get; set; }
    public bool WasMapped { get; set; }
    public bool WasFootfalled { get; set; }
    public string PlanetClass { get; set; } = string.Empty;
    public bool Landable { get; set; }
    public bool Mapped { get; set; }
    public string TerraformState { get; set; } = string.Empty;
    public double DistanceFromArrivalLS { get; set; }
    public List<SignalCard> Signals { get; set; } = [];
}
