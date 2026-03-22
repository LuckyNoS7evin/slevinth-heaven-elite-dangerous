namespace SlevinthHeavenEliteDangerous.Eddn;

public sealed class EddnOptions
{
    public bool Enabled { get; set; } = false;
    public bool TestMode { get; set; } = true;
    public string SoftwareName { get; set; } = "Slevinth Heaven Elite Dangerous";
    public string SoftwareVersion { get; set; } = "1.0.0";
    public string Endpoint { get; set; } = "https://eddn.edcd.io:4430/upload/";

    /// <summary>
    /// When true, systems are looked up in the community database before deciding whether to hold
    /// or immediately release pending nav events. Known systems are released immediately;
    /// unknown systems remain held until SellExplorationData.
    /// </summary>
    public bool SystemLookupEnabled { get; set; } = true;

    /// <summary>
    /// Base URL for the system lookup API.
    /// Must accept a <c>?sysId64={systemAddress}</c> query parameter and return <c>{}</c>
    /// (empty object) when the system is not found, or a non-empty JSON object when found.
    /// Default: EDSM system endpoint.
    /// </summary>
    public string SystemLookupApiUrl { get; set; } = "https://www.edsm.net/api-v1/system";

    /// <summary>
    /// Absolute path to the directory containing per-FID journal subdirectories.
    /// Set programmatically from IWebHostEnvironment at startup — not read from config.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;
}
