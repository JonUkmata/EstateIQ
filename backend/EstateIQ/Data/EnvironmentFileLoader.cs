namespace EstateIQ.Data;

public static class EnvironmentFileLoader
{
    public static void Load(string basePath)
    {
        var originalEnvironmentKeys = Environment.GetEnvironmentVariables()
            .Keys
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        LoadFile(Path.Combine(basePath, ".env"), originalEnvironmentKeys, allowOverrideLoadedValues: false);
        LoadFile(Path.Combine(basePath, ".env.local"), originalEnvironmentKeys, allowOverrideLoadedValues: true);
    }

    private static void LoadFile(string filePath, HashSet<string> originalEnvironmentKeys, bool allowOverrideLoadedValues)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value[1..^1];
            }

            if (originalEnvironmentKeys.Contains(key))
            {
                continue;
            }

            if (!allowOverrideLoadedValues && Environment.GetEnvironmentVariable(key) is not null)
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
