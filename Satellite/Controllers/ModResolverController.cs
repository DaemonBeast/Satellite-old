/*using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Satellite.Controllers;

[ApiController]
[Route("mods/resolve/{id}")]
public class ModResolverController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ModResolverController(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    [HttpGet]
    public async Task<ActionResult> GetDownloadLinks(
        string id,
        [FromQuery(Name = "version")] string? version,
        [FromQuery(Name = "redirect")] bool redirect)
    {
        var database = await this._httpClient.GetFromJsonAsync<ModsInfo>(Constants.DatabaseUrl);

        if (database == null)
        {
            return this.StatusCode(500);
        }

        if (!database.Mods.TryGetValue(id, out var modInfo))
        {
            return this.NotFound();
        }
        
        
    }

    private async Task<VersionReference[]> ResolveReferences(ModsInfo database, ModInfo modInfo, string? version)
    {
        var providers = new List<VersionReference>();

        switch (modInfo.Provider.Type)
        {
            case "link":
            {
                var modFile = await this._httpClient.GetFromJsonAsync<ModFile>(modInfo.Provider.Value);
                if (modFile == null)
                {
                    throw new ModFileMissingException(
                        $"The mod file \"{modInfo.Provider.Value}\" is invalid or missing.");
                }

                ModVersion? modVersion;

                if (version == null)
                {
                    if (modFile.Versions.Count == 0)
                    {
                        throw new ModFileNoVersionsException(
                            $"The mod file \"{modInfo.Provider.Value}\" has no versions.");
                    }

                    var maxVersion = modFile.Versions.Keys.Max()!;
                    modVersion = modFile.Versions[maxVersion];
                    
                    providers.Add(new LinkVersionReference(modVersion.DownloadLink));

                    if (modVersion.Dependencies != null)
                    {
                        foreach (var (dependencyName, dependencyVersion) in modVersion.Dependencies)
                        {
                            if (!database.Mods.TryGetValue(dependencyName, out var dependencyInfo))
                            {
                                throw new UnknownModException(
                                    $"The specified dependency \"{dependencyName}\" is not in the database.");
                            }

                            providers.AddRange(
                                await this.ResolveReferences(database, dependencyInfo, dependencyVersion));
                        }
                    }

                    return providers.ToArray();
                }

                if (!modFile.Versions.TryGetValue(version, out modVersion))
                {
                    throw new ModFileInvalidVersionException(
                        $"The specified version \"{version}\" does not exist in the mod file \"{modInfo.Provider.Value}\".");
                }
                
                providers.Add(new LinkVersionReference(modVersion.DownloadLink));
                
                if (modVersion.Dependencies != null)
                {
                    foreach (var (dependencyName, dependencyVersion) in modVersion.Dependencies)
                    {
                        if (!database.Mods.TryGetValue(dependencyName, out var dependencyInfo))
                        {
                            throw new UnknownModException(
                                $"The specified dependency \"{dependencyName}\" is not in the database.");
                        }

                        providers.AddRange(
                            await this.ResolveReferences(database, dependencyInfo, dependencyVersion));
                    }
                }
                
                return providers.ToArray();
            }
            case "server":
            {
                throw new NotImplementedException();
            }
            default:
            {
                throw new InvalidProviderTypeException(
                    $"Invalid provider type \"{modInfo.Provider.Type}\" was encountered.");
            }
        }
    }

    private class VersionReferences
    {
        [JsonPropertyName("references")]
        public VersionReference[]
    }

    private class LinkVersionReference : VersionReference
    {
        [JsonPropertyName("link")]
        public string Link { get; }

        public LinkVersionReference(string link)
        {
            this.Link = link;
        }
    }
    
    private class ServerVersionReference : VersionReference
    {
        [JsonPropertyName("url")]
        public string Url { get; }
        
        [JsonPropertyName("version")]
        public string Version { get; }
        
        [JsonPropertyName("auth_token")]
        public string? AuthToken { get; }

        public ServerVersionReference(string url, string version, string? authToken = null)
        {
            this.Url = url;
            this.Version = version;
            this.AuthToken = authToken;
        }
    }

    private class VersionReference {}

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
    
    private class InvalidProviderTypeException : Exception
    {
        public InvalidProviderTypeException(string message) : base(message) {}
    }
    
    private class UnknownModException : Exception
    {
        public UnknownModException(string message) : base(message) {}
    }

    private class ModFileMissingException : Exception
    {
        public ModFileMissingException(string message) : base(message) {}
    }
    
    private class ModFileNoVersionsException : Exception
    {
        public ModFileNoVersionsException(string message) : base(message) {}
    }
    
    private class ModFileInvalidVersionException : Exception
    {
        public ModFileInvalidVersionException(string message) : base(message) {}
    }
}*/