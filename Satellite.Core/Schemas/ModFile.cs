using System.Text.Json.Serialization;

namespace Satellite.Core.Schemas;

public class ModFile
{
    /*[JsonPropertyName("private")]
    public bool Private { get; set; } = false;*/

    [JsonPropertyName("versions")]
    public Dictionary<string, ModVersion> Versions { get; set; } = new();
}

public class ModVersion
{
    [JsonPropertyName("game_version")]
    public string GameVersion { get; set; } = "*";
    
    [JsonPropertyName("download_link")]
    public string DownloadLink { get; set; } = string.Empty;
    
    [JsonPropertyName("dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; } = null;
}