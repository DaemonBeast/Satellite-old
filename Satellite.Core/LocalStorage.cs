using Satellite.Core.Interfaces;

namespace Satellite.Core;

public class LocalStorage : IStorage
{
    public Task<Stream> ReadAsync(string filePath)
    {
        return Task.FromResult<Stream>(new FileInfo(filePath).OpenRead());
    }

    public async Task WriteAsync(string filePath, Stream data)
    {
        await data.CopyToAsync(new FileInfo(filePath).OpenWrite());
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }
}