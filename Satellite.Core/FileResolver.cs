using Satellite.Core.Interfaces;

namespace Satellite.Core;

public class FileResolver
{
    private readonly HttpClient _httpClient;
    private readonly IStorage _storage;

    public FileResolver(HttpClient httpClient, IStorage storage)
    {
        this._httpClient = httpClient;
        this._storage = storage;
    }

    public async Task<Stream> GetStreamAsync(string filePath)
    {
        return filePath.StartsWith("http")
            ? await this._httpClient.GetStreamAsync(filePath)
            : await this._storage.ReadAsync(filePath);
    }
}