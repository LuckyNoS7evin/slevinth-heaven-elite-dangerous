using Refit;
using SlevinthHeavenEliteDangerous.Core.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Refit interface for the Slevinth Heaven API.
/// Authentication is handled automatically by <see cref="FrontierAuthHandler"/>.
/// </summary>
public interface ISlevinthHeavenApi
{
    [Get("/weatherforecast")]
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();

    [Get("/version")]
    Task<VersionInfo> GetVersionAsync();

    [Post("/version/report")]
    Task ReportVersionAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form);

    [Post("/diagnostics")]
    Task PostDiagnosticsAsync([Body] DiagnosticsReport report);

    /// <summary>
    /// Upload a raw journal file to the server for background processing.
    /// Content should be multipart/form-data with "file" and "fid" parts.
    /// </summary>
    [Post("/journal/upload")]
    Task UploadJournalFileAsync([Body] HttpContent content, CancellationToken ct = default);

    /// <summary>
    /// Upload a companion file (Market.json, Shipyard.json, etc.) for EDDN forwarding.
    /// Content should be multipart/form-data with "type", "fid", and "file" parts.
    /// </summary>
    [Post("/companion/upload")]
    Task UploadCompanionFileAsync([Body] HttpContent content, CancellationToken ct = default);
}
