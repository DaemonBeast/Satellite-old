namespace Satellite.Core.Settings;

public class DatabaseSettings
{
    public const string Section = "Database";
    
    public string[] Databases { get; set; } = Array.Empty<string>();
}