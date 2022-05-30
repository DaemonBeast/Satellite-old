using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Satellite.Controllers;

[ApiController]
[Route("bepinex/{backend}")]
public class BepInExController : ControllerBase
{
    private readonly HttpClient _httpClient;

    private const string BaseBepInExPath = "https://builds.bepinex.dev";
    private const string BepInExProjectId = "bepinex_be";

    private readonly string _baseBepInExApiPath = Path.Join(BaseBepInExPath, "/api");

    private readonly Dictionary<string, string> _fileBackendMappings = new()
    {
        { "mono", "UnityMono" },
        { "il2cpp", "UnityIL2CPP" },
        { "net_launcher", "NetLauncher" }
    };

    private readonly Dictionary<string, string[]?> _validBackendArchs = new()
    {
        { "mono", new[] { "x86", "x64", "unix" } },
        { "il2cpp", new[] { "x86", "x64" } },
        { "net_launcher", null }
    };

    public BepInExController(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }
    
    [HttpGet]
    public async Task<ActionResult> GetDownloadLink(
        string backend,
        [FromQuery(Name = "build_id")] string? buildId,
        [FromQuery(Name = "redirect")] bool redirect)
    {
        return await GetDownloadLink(backend, null, buildId ?? "latest", redirect);
    }
    
    [HttpGet("{arch}")]
    public async Task<ActionResult> GetDownloadLink(
        string backend,
        string? arch,
        [FromQuery(Name = "build_id")] string? buildId,
        [FromQuery(Name = "redirect")] bool redirect)
    {
        string downloadUri;
        
        try
        {
            downloadUri = await this.GetBepInExDownloadLink(buildId ?? "latest", backend, arch);
        }
        catch (InvalidBepInExRequestException e)
        {
            return this.BadRequest(new ErrorResponse(e));
        }
        catch (NotFoundBepInExRequestException e)
        {
            return this.NotFound(new ErrorResponse(e));
        }
        catch
        {
            return this.StatusCode(500);
        }

        return redirect ? this.Redirect(downloadUri) : this.Ok(new DownloadLinkResponse(downloadUri));
    }

    private async Task<string> GetBepInExDownloadLink(string buildId, string backend, string? arch = null)
    {
        var fileSegment = this.GetFileSegment(backend, arch);
        if (fileSegment == null)
        {
            throw new InvalidBepInExRequestException($"Invalid architecture \"{arch}\" or backend \"{backend}\".");
        }

        var requestUri = Path.Join(_baseBepInExApiPath, $"/projects/{BepInExProjectId}/artifacts/{buildId}");
        ArtifactsInfo? artifactsInfo;

        try
        {
            artifactsInfo = await this._httpClient.GetFromJsonAsync<ArtifactsInfo>(requestUri);
        }
        catch
        {
            throw new NotFoundBepInExRequestException($"Invalid build id \"{buildId}\".");
        }

        var file = artifactsInfo!.Artifacts
            .Select(artifact => artifact.File)
            .First(file => file.Contains(fileSegment));
        
        return Path.Join(BaseBepInExPath, $"/projects/{BepInExProjectId}/{artifactsInfo.Id}/{file}");
    }

    private string? GetFileSegment(string backend, string? arch = null)
    {
        var lowercaseBackend = backend.ToLowerInvariant();
        var lowercaseArch = arch?.ToLowerInvariant();
        
        if (!this._fileBackendMappings.TryGetValue(lowercaseBackend, out var backendId))
        {
            return null;
        }

        var validArchs = _validBackendArchs[lowercaseBackend];
        if (validArchs == null)
        {
            return backendId;
        }

        if (lowercaseArch == null || !validArchs.Contains(lowercaseArch))
        {
            return null;
        }

        return $"{backendId}_{lowercaseArch}";
    }

    private class DownloadLinkResponse
    {
        public string Link { get; }

        public DownloadLinkResponse(string link)
        {
            this.Link = link;
        }
    }

    private class ErrorResponse
    {
        public string Error { get; }

        public ErrorResponse(Exception e)
        {
            this.Error = e.Message;
        }
    }

    private class ArtifactsInfo
    {
        [JsonPropertyName("artifacts")]
        public ArtifactInfo[] Artifacts { get; set; }
        
        [JsonPropertyName("changelog")]
        public string Changelog { get; set; }
        
        [JsonPropertyName("date")]
        public string Date { get; set; }
        
        [JsonPropertyName("hash")]
        public string Hash { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("short_hash")]
        public string ShortHash { get; set; }

        public ArtifactsInfo(
            ArtifactInfo[] artifacts,
            string changelog,
            string date,
            string hash,
            string id,
            string shortHash)
        {
            this.Artifacts = artifacts;
            this.Changelog = changelog;
            this.Date = date;
            this.Hash = hash;
            this.Id = id;
            this.ShortHash = shortHash;
        }
    }
    
    private class ArtifactInfo
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("file")]
        public string File { get; set; }

        public ArtifactInfo(string description, string file)
        {
            this.Description = description;
            this.File = file;
        }
    }

    private class InvalidBepInExRequestException : Exception
    {
        public InvalidBepInExRequestException(string message) : base(message) {}
    }
    
    private class NotFoundBepInExRequestException : Exception
    {
        public NotFoundBepInExRequestException(string message) : base(message) {}
    }
}