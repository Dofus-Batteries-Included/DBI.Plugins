using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Metadata;

public class BuildMetadataHelpers
{
    public static Guid? ReadDofusBuildId(ILogger logger)
    {
        const string bootConfigFilePathRelativeToDofusExe = "Dofus_Data/boot.config";
        string dofusExePath = FindDofusExePath();
        string bootConfigPath = Path.Join(dofusExePath, bootConfigFilePathRelativeToDofusExe);
        if (!File.Exists(bootConfigPath))
        {
            logger.LogWarning("Could not find boot.config, looked at: {Path}", bootConfigPath);
            return null;
        }

        logger.LogDebug("Found boot.config at {Path}", bootConfigPath);

        string[] lines = File.ReadAllLines(bootConfigPath);
        Dictionary<string, string> props = new();
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            props[parts[0]] = parts[1];
        }

        string guidStr = props.GetValueOrDefault("build-guid");
        if (!Guid.TryParse(guidStr, out Guid guid))
        {
            logger.LogWarning("Could not find build-guid in boot.config file (path: {Path})", bootConfigPath);
            return null;
        }

        return guid;
    }

    public static Guid? GetExpectedBuildIdFromAssemblyAttribute<T>() => typeof(T).Assembly.GetCustomAttribute<ExpectedDofusBuildIdAttribute>()?.BuildId;

    public static string GetExpectedVersionFromAssemblyAttribute<T>() => typeof(T).Assembly.GetCustomAttribute<ExpectedDofusVersionAttribute>()?.Version;

    static string FindDofusExePath()
    {
        string current = Path.GetDirectoryName(Path.GetFullPath(typeof(BuildMetadataHelpers).Assembly.Location));
        while (current != null && !File.Exists(Path.Join(current, "Dofus.exe")))
        {
            current = Path.GetDirectoryName(current);
        }

        return current;
    }
}
