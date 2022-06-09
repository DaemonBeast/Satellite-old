using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Satellite.Core.Interfaces;
using Satellite.S3.Settings;

namespace Satellite.S3.Extensions;

public static class S3ServiceExtensions
{
    public static IHostBuilder AddSatelliteS3(this IHostBuilder builder)
    {
        // should only be necessary for development environment
        Env.Load();
        
        builder.ConfigureServices((host, services) =>
        {
            var s3SettingsSection = host.Configuration.GetSection(S3Settings.Section);
            services.Configure<S3Settings>(s3SettingsSection);

            var s3Settings = s3SettingsSection.Get<S3Settings>() ?? new S3Settings();

            var accessKeyId = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
            var secretAccessKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");

            services.AddAWSService<IAmazonS3>(new AWSOptions
            {
                Credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey),
                DefaultClientConfig =
                {
                    ServiceURL = s3Settings.ServiceUrl
                }
            });

            services.AddSingleton<IStorage, S3Storage>();
        });

        return builder;
    }
}