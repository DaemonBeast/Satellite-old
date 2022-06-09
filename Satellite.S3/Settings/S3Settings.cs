namespace Satellite.S3.Settings;

public class S3Settings
{
    public const string Section = "S3";

    public bool Enabled { get; set; }

    public string ServiceUrl { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;
}