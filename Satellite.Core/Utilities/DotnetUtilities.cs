using System.Reflection;

namespace Satellite.Core.Utilities;

public static class DotnetUtilities
{
    public const string DefaultVersion = "1.0.0";
    public const string DefaultName = "Program";

    public static string Name => Assembly.GetCallingAssembly().GetName().Name ?? DefaultName;

    public static string Version => Assembly.GetCallingAssembly()
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        ?.InformationalVersion
                                    ?? DefaultVersion;
}