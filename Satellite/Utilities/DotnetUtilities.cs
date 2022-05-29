using System.Reflection;

namespace Satellite.Utilities;

public static class DotnetUtilities
{
    public const string DefaultVersion = "1.0.0";
    public const string DefaultName = "Server";

    public static string Name => _name ??= typeof(Program).Assembly.GetName().Name ?? DefaultName;
    
    public static string Version =>
        _version ??=
            typeof(DotnetUtilities)
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? DefaultVersion;

    private static string? _name;
    private static string? _version;
}