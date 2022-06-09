using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Satellite.Core.Interfaces;
using Satellite.S3.Settings;

namespace Satellite.S3;

public class S3Storage : IStorage
{
    private readonly S3Settings _s3Settings;
    private readonly IAmazonS3 _s3Client;

    public S3Storage(IOptions<S3Settings> s3Settings, IAmazonS3 s3Client)
    {
        this._s3Settings = s3Settings.Value;
        this._s3Client = s3Client;
    }

    public async Task<Stream> ReadAsync(string filePath)
    {
        var s3Object = await this._s3Client.GetObjectAsync(this._s3Settings.BucketName, filePath);

        return s3Object.ResponseStream;
    }

    public async Task WriteAsync(string filePath, Stream data)
    {
        await this.CreateBucketIfNotExists();

        var request = new PutObjectRequest
        {
            BucketName = this._s3Settings.BucketName,
            InputStream = data,
            Key = filePath
        };

        await this._s3Client.PutObjectAsync(request);
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        await this.CreateBucketIfNotExists();

        try
        {
            await this._s3Client.GetObjectMetadataAsync(this._s3Settings.BucketName, filePath);

            return true;
        }
        catch (AmazonS3Exception e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            
            throw;
        }
    }

    private async Task CreateBucketIfNotExists()
    {
        if (!await this._s3Client.DoesS3BucketExistAsync(this._s3Settings.BucketName))
        {
            await this._s3Client.PutBucketAsync(this._s3Settings.BucketName);
        }
    }
}