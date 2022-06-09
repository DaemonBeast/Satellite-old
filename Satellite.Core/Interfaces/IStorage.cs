namespace Satellite.Core.Interfaces;

public interface IStorage
{
    public Task<Stream> ReadAsync(string filePath);
    
    public Task WriteAsync(string filePath, Stream data);

    public Task<bool> FileExistsAsync(string filePath);
}