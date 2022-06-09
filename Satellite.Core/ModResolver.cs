using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Versioning;
using Satellite.Core.Schemas;
using Satellite.Core.Settings;

namespace Satellite.Core;

public class ModResolver
{
    private readonly ILogger<ModResolver> _logger;
    private readonly DatabaseSettings _databaseSettings;
    private readonly HttpClient _httpClient;
    private readonly FileResolver _fileResolver;

    private readonly List<DatabaseFile> _databases;
    private DateTimeOffset _lastUpdated;

    private const int ExpireInterval = 5;
    
    // TODO: mod version caching

    public ModResolver(
        ILogger<ModResolver> logger,
        IOptions<DatabaseSettings> databaseSettings,
        HttpClient httpClient,
        FileResolver fileResolver)
    {
        this._logger = logger;
        this._databaseSettings = databaseSettings.Value;
        this._httpClient = httpClient;
        this._fileResolver = fileResolver;
        
        this._databases = new List<DatabaseFile>();
        this._lastUpdated = DateTimeOffset.UnixEpoch;
    }

    public async Task<ModInfo?> GetModInfo(string modId)
    {
        await this.UpdateCacheIfNecessary();
        
        return this._databases.FirstOrDefault(database => database.Mods.ContainsKey(modId))?.Mods[modId];
    }

    public async Task<Stream?> GetDownloadStream(
        string id,
        ModInfo modInfo,
        string? version = null,
        string? gameVersion = null)
    {
        var modVersion = await this.GetModVersion(modInfo, version, gameVersion);

        return modVersion == null
            ? await this.GetDownloadServerStream(id, modInfo, version, gameVersion)
            : await this._fileResolver.GetStreamAsync(modVersion.DownloadLink);
    }

    public async Task<ModVersion?> GetModVersion(ModInfo modInfo, string? version = null, string? gameVersion = null)
    {
        var semanticVersion = gameVersion == null ? null : NuGetVersion.Parse(gameVersion);
        
        SemanticVersion? maxVersion = null;
        ModVersion? maxModVersion = null;
        
        foreach (var modFileUrl in modInfo.ModFileUrls)
        {
            ModFile? modFile;
            
            try
            {
                modFile = await this._httpClient.GetFromJsonAsync<ModFile>(modFileUrl);
            }
            catch
            {
                continue;
            }

            if (modFile == null || modFile.Versions.Count == 0)
            {
                continue;
            }
            
            if (version == null)
            {
                var modFileVersions = modFile.Versions
                    .Where(v => semanticVersion == null ||
                                VersionRange.Parse(v.Value.GameVersion).Satisfies(semanticVersion))
                    .ToDictionary(v => NuGetVersion.Parse(v.Key), v => v.Key);
                
                var modFileMaxVersion = modFileVersions.Keys.Max();
                if (modFileMaxVersion == null)
                {
                    continue;
                }
                
                if (maxVersion == null || modFileMaxVersion > maxVersion)
                {
                    maxVersion = modFileMaxVersion;
                    maxModVersion = modFile.Versions[modFileVersions[modFileMaxVersion]];
                }
            }
            else
            {
                if (modFile.Versions.TryGetValue(version, out var modVersion))
                {
                    return modVersion;
                }
            }
        }

        return maxModVersion;
    }
    
    public async Task<Stream?> GetDownloadServerStream(
        string id,
        ModInfo modInfo,
        string? version = null,
        string? gameVersion = null)
    {
        var query = new QueryString();

        if (version != null)
        {
            query.Add("version", version);
        }

        if (gameVersion != null)
        {
            query.Add("game_version", gameVersion);
        }

        var url = new UriBuilder
        {
            Path = $"mods/download/{id}",
            Query = query.ToString()
        };

        foreach (var downloadServerUrl in modInfo.DownloadServerUrls)
        {
            try
            {
                url.Host = downloadServerUrl;
                return await this._httpClient.GetStreamAsync(url.ToString());
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    private async Task UpdateCacheIfNecessary()
    {
        if (DateTimeOffset.UtcNow - this._lastUpdated > TimeSpan.FromMinutes(ExpireInterval))
        {
            this._lastUpdated = DateTimeOffset.UtcNow;

            var requests = this._databaseSettings.Databases
                .Select(databaseUrl =>
                    this._httpClient
                        .GetFromJsonAsync<DatabaseFile>(databaseUrl)
                        .ContinueWith(task =>
                        {
                            if (task.Result == null)
                            {
                                this._logger.LogWarning("Failed to load database \"{DatabaseUrl}\"", databaseUrl);
                            }
                            else
                            {
                                this._databases.Add(task.Result);
                            }
                        }));

            await Task.WhenAll(requests);
        }
    }
}