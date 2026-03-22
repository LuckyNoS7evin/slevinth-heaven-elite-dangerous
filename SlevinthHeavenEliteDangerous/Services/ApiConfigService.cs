namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Provides API connection details compiled into the binary via Resource1.resx.
/// No file loading required — values are available immediately at startup.
/// </summary>
public class ApiConfigService
{
    public string BaseUrl { get; } = AppResources.ApiBaseUrl;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        BaseUrl != "https://your-api-url.com";
}
