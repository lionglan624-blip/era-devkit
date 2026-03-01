using System;
using System.IO;

namespace Era.DevKit.TestUtils;

/// <summary>
/// Resolves paths to the Game data directory across all devkit test projects.
/// Uses GAME_PATH environment variable when set, otherwise falls back to
/// relative path navigation from the assembly or working directory.
/// </summary>
internal static class GamePathHelper
{
    private static readonly Lazy<string> _gamePath = new(ResolveGamePath);

    /// <summary>
    /// Root path to the Game data directory.
    /// </summary>
    public static string GameRoot => _gamePath.Value;

    /// <summary>
    /// Resolves a path relative to the Game root directory.
    /// Example: Resolve("CSV", "Talent.csv") -> "C:\Era\game\CSV\Talent.csv"
    /// </summary>
    public static string Resolve(params string[] subPaths)
    {
        var parts = new string[subPaths.Length + 1];
        parts[0] = GameRoot;
        Array.Copy(subPaths, 0, parts, 1, subPaths.Length);
        return Path.Combine(parts);
    }

    private static string ResolveGamePath()
    {
        // 1. GAME_PATH env var (standalone repo mode)
        var envPath = Environment.GetEnvironmentVariable("GAME_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
            return Path.GetFullPath(envPath);

        // 2. Walk up from working directory to find Game/ (monorepo mode)
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "Game");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }

        // 3. Relative from assembly (legacy fallback: 7 levels up for tools)
        var assemblyDir = AppContext.BaseDirectory;
        var fallback = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "..", "..", "Game"));
        return fallback;
    }
}
