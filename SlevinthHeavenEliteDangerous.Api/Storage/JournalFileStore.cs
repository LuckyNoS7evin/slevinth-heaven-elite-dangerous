namespace SlevinthHeavenEliteDangerous.Api.Storage;

/// <summary>
/// Stores raw journal files on disk at <c>Data/Journals/{FID}/{filename}</c>.
/// Files are saved verbatim and kept for background processing and reprocessing.
/// </summary>
public sealed class JournalFileStore
{
    private readonly string _basePath;

    public JournalFileStore(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.ContentRootPath, "Data", "Journals");
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Save a raw journal file for the given commander.
    /// Overwrites if the file already exists (re-upload with new content).
    /// </summary>
    public async Task SaveFileAsync(string fid, string fileName, Stream content)
    {
        fid = Sanitise(fid);
        fileName = SanitiseFileName(fileName);

        var dir = Path.GetFullPath(Path.Combine(_basePath, fid));
        if (!dir.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid FID: path traversal detected.");

        var filePath = Path.GetFullPath(Path.Combine(dir, fileName));
        if (!filePath.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid file name: path traversal detected.");

        Directory.CreateDirectory(dir);

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs);
    }

    /// <summary>
    /// Get all FID directories that contain journal files.
    /// </summary>
    public IEnumerable<string> ListFIDs()
    {
        if (!Directory.Exists(_basePath))
            return [];

        return Directory.GetDirectories(_basePath)
            .Select(Path.GetFileName)
            .Where(f => !string.IsNullOrEmpty(f))!;
    }

    /// <summary>
    /// Get the directory path for a commander's journal files.
    /// </summary>
    public string GetCommanderDirectory(string fid) =>
        Path.Combine(_basePath, Sanitise(fid));

    /// <summary>
    /// Get all journal files for a commander, sorted by name (chronological).
    /// </summary>
    public IReadOnlyList<string> GetJournalFiles(string fid)
    {
        var dir = GetCommanderDirectory(fid);
        if (!Directory.Exists(dir))
            return [];

        return Directory.GetFiles(dir, "*.log")
            .OrderBy(Path.GetFileName)
            .ToList();
    }

    private static string Sanitise(string fid) =>
        string.Concat(fid.Where(c => char.IsLetterOrDigit(c) || c == '-'));

    private static string SanitiseFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitised = string.Concat(name.Where(c => !invalid.Contains(c)));
        return sanitised.Length > 128 ? sanitised[..128] : sanitised;
    }
}
