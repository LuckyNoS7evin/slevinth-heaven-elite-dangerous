using System.Text.Json.Nodes;

namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Represents a fully-formed EDDN message ready for serialisation and upload.
/// </summary>
public sealed class EddnMessage
{
    public required string SchemaRef { get; init; }
    public required string UploaderID { get; init; }
    public required string SoftwareName { get; init; }
    public required string SoftwareVersion { get; init; }
    public required string GameVersion { get; init; }
    public required string GameBuild { get; init; }

    /// <summary>Already-sanitised message body as a JSON string.</summary>
    public required string MessageJson { get; init; }

    /// <summary>
    /// Assembles the final EDDN payload.
    /// In test mode, "/test" is appended to the schema ref.
    /// </summary>
    public string ToJson(bool testMode)
    {
        var schemaRef = testMode ? SchemaRef + "/test" : SchemaRef;

        var obj = new JsonObject
        {
            ["$schemaRef"] = schemaRef,
            ["header"] = new JsonObject
            {
                ["uploaderID"] = UploaderID,
                ["softwareName"] = SoftwareName,
                ["softwareVersion"] = SoftwareVersion,
                ["gameversion"] = GameVersion,
                ["gamebuild"] = GameBuild,
            },
            ["message"] = JsonNode.Parse(MessageJson),
        };

        return obj.ToJsonString();
    }
}
