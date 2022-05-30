using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Satellite.Controllers;

[ApiController]
[Route("mods/download/{mod}")]
public class ModDownloadController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ModDownloadController(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    [HttpGet]
    public async Task<ActionResult> GetDownloadLink(
        string mod,
        [FromQuery(Name = "version")] string? version,
        [FromQuery(Name = "redirect")] bool redirect)
    {
        var database = await this._httpClient.GetFromJsonAsync<ModsInfo>(Constants.DatabaseUrl);

        if (database == null)
        {
            return this.StatusCode(500);
        }

        if (!database.Mods.TryGetValue(mod, out var modInfo))
        {
            return this.NotFound();
        }

        switch (modInfo.Provider.Type)
        {
            case "link":
            {
                var modFile = await this._httpClient.GetFromJsonAsync<ModFile>(modInfo.Provider.Value);
                if (modFile == null)
                {
                    return this.StatusCode(500);
                }

                if (version == null)
                {
                    // TODO: implement proper version check, as first listed version may not be latest
                    var downloadLink = modFile.Versions.Values.ElementAt(0).DownloadLink;
                    
                    return redirect ? this.Redirect(downloadLink) : this.Ok(downloadLink);
                }

                if (!modFile.Versions.TryGetValue(version, out var modVersion))
                {
                    return this.StatusCode(500);
                }
                
                return redirect ? this.Redirect(modVersion.DownloadLink) : this.Ok(modVersion.DownloadLink);
            }
            case "server":
            {
                return this.StatusCode(501);
            }
            default:
            {
                return this.StatusCode(500);
            }
        }
    }

    private class ModsInfo
    {
        [JsonPropertyName("mods")]
        public Dictionary<string, ModInfo> Mods { get; set; }

        public ModsInfo(Dictionary<string, ModInfo> mods)
        {
            this.Mods = mods;
        }
    }

    private class ModInfo
    {
        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        public ModInfo(Provider provider)
        {
            this.Provider = provider;
        }
    }

    private class Provider
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public Provider(string type, string value)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    private class ModFile
    {
        [JsonPropertyName("versions")]
        public Dictionary<string, ModVersion> Versions { get; set; }

        public ModFile(Dictionary<string, ModVersion> versions)
        {
            this.Versions = versions;
        }
    }

    private class ModVersion
    {
        [JsonPropertyName("among_us_versions")]
        public string[] AmongUsVersions { get; set; }
        
        [JsonPropertyName("download_link")]
        public string DownloadLink { get; set; }
        
        [JsonPropertyName("dependencies")]
        public Dictionary<string, string>? Dependencies { get; set; }

        public ModVersion(string[] amongUsVersions, string downloadLink, Dictionary<string, string>? dependencies)
        {
            this.AmongUsVersions = amongUsVersions;
            this.DownloadLink = downloadLink;
            this.Dependencies = dependencies;
        }
    }
}