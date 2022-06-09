using System.Text.Json.Serialization;

namespace Satellite.Core.Schemas;

public class DatabaseFile
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("mods")]
    public Dictionary<string, ModInfo> Mods { get; set; } = new();
}

public class ModInfo
{
    /*[JsonPropertyName("providers")]
    public Provider[] Providers { get; set; } = Array.Empty<Provider>();*/
    
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("download_name")]
    public string DownloadName { get; set; } = string.Empty;

    [JsonPropertyName("mod_file_urls")]
    public string[] ModFileUrls { get; set; } = Array.Empty<string>();

    [JsonPropertyName("download_server_urls")]
    public string[] DownloadServerUrls { get; set; } = Array.Empty<string>();
}

/*public class Provider
{
    [JsonPropertyName("mod_file_url")]
    public string? ModFileUrl { get; set; } = null;

    [JsonPropertyName("download_server_url")]
    public string? DownloadServerUrl { get; set; } = null;
}*/