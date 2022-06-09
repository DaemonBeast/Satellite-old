using System.Text.Json.Serialization;

namespace Satellite.Core.Schemas.Responses;

public class DownloadLinkResponse
{
    [JsonPropertyName("download_link")]
    public string DownloadLink { get; set; } = string.Empty;

    [JsonPropertyName("dependency_links")]
    public string[]? DependencyLinks { get; set; } = null;
}