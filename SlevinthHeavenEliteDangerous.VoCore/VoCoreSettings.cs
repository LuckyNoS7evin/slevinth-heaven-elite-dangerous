using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.VoCore;

/// <summary>
/// Persisted settings for the VoCore display connection.
/// Stored at Documents\SlevinthHeavenEliteDangerous\vocore_settings.json.
///
/// VendorId and ProductId are stored as hex strings (e.g. "0xABCD") so the file
/// is human-readable and easy to edit without needing a decimal converter.
/// If the file is absent, defaults are written so the file can be found and edited.
/// Note: the display must use the WinUSB or LibUSB-win32 driver — LibUSB-K is not supported
/// by LibUsbDotNet 2.x. Use Zadig to switch from LibUSB-K to WinUSB if needed.
/// </summary>
public sealed class VoCoreSettings
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "SlevinthHeavenEliteDangerous",
        "vocore_settings.json");

    /// <summary>USB Vendor ID as a hex string, e.g. "0xABCD".</summary>
    [JsonPropertyName("VendorId")]
    public string VendorIdHex { get; set; } = "0xC872";

    /// <summary>USB Product ID as a hex string, e.g. "0x1234".</summary>
    [JsonPropertyName("ProductId")]
    public string ProductIdHex { get; set; } = "0x1004";

    /// <summary>Display width in pixels (portrait mode).</summary>
    [JsonPropertyName("Width")]
    public int Width { get; set; } = 480;

    /// <summary>Display height in pixels (portrait mode).</summary>
    [JsonPropertyName("Height")]
    public int Height { get; set; } = 854;

    /// <summary>
    /// Number of pixels to circular-shift each row left before sending to the display.
    /// The VoCore Screen framebuffer origin is offset from pixel 0 — this corrects it.
    /// Increase to shift content left, decrease to shift right. 
    /// Portrait mode (480x854): Default 325 (empirically determined)
    /// Landscape mode (854x480): Default 240 (empirically determined)
    /// </summary>
    [JsonPropertyName("ColumnOffset")]
    public int ColumnOffset { get; set; } = 325;

    /// <summary>
    /// Set to true to skip the vendor control transfers and send raw bulk data directly.
    /// Useful for diagnosing whether the control transfer or frame size is the problem.
    /// </summary>
    [JsonPropertyName("SkipWindowCommand")]
    public bool SkipWindowCommand { get; set; } = false;

    /// <summary>
    /// Set to true if the display shows corrupted colours — swaps each pair of bytes in the
    /// RGB565 frame from SkiaSharp's native little-endian order to big-endian before sending.
    /// </summary>
    [JsonPropertyName("SwapByteOrder")]
    public bool SwapByteOrder { get; set; } = false;

    [JsonIgnore]
    public int VendorId => ParseHex(VendorIdHex, 0xC872);

    [JsonIgnore]
    public int ProductId => ParseHex(ProductIdHex, 0x1004);

    /// <summary>
    /// Loads settings from disk, returning defaults if the file does not exist or cannot be read.
    /// Creates the file with defaults if absent so the user can find and edit it.
    /// </summary>
    public static VoCoreSettings Load()
    {
        var path = FilePath;
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<VoCoreSettings>(json);
                if (loaded != null)
                {
                    Debug.WriteLine($"[VoCore] Settings loaded — VID={loaded.VendorIdHex} PID={loaded.ProductIdHex} {loaded.Width}x{loaded.Height}");
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoCore] Could not read settings: {ex.Message} — using defaults");
        }

        // Write defaults so the user can discover and edit the file.
        var defaults = new VoCoreSettings();
        defaults.Save();
        return defaults;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, _jsonOptions));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoCore] Could not save settings: {ex.Message}");
        }
    }

    private static int ParseHex(string value, int fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        try
        {
            string s = value.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return Convert.ToInt32(s[2..], 16);
            return Convert.ToInt32(s, 16);
        }
        catch
        {
            return fallback;
        }
    }
}
