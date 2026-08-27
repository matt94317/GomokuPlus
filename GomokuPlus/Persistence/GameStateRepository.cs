using System.Text.Json;

namespace GomokuPlus.Persistence;

// Reads/writes a GameState as JSON. The only file-I/O boundary in the
// persistence layer - GameStateMapper and GameState stay I/O-free so they
// can be exercised without touching disk.
public static class GameStateRepository
{
    // A bare filename like "SAVE:mygame" lands here rather than next to
    // the source files - keeps player-generated save data out of the code
    // directories. An explicit path (one with a directory component) is
    // still respected as-is, so this is a default, not a restriction.
    private const string DefaultSaveDirectory = "SavedGames";

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    // Returns the resolved path actually written to, so callers can tell
    // the player where the save landed rather than echoing their raw input.
    public static string Save(string path, GameState state)
    {
        var resolved = ResolvePath(path);
        var directory = Path.GetDirectoryName(resolved);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(state, Options);
        File.WriteAllText(resolved, json);
        return resolved;
    }

    public static GameState Load(string path)
    {
        var json = File.ReadAllText(ResolvePath(path));
        return JsonSerializer.Deserialize<GameState>(json)
            ?? throw new InvalidDataException("Save file is empty or malformed.");
    }

    // Callers type a bare name like "mygame" as often as a full filename -
    // treat ".json" as the implied extension either way, and a bare name
    // (no directory component) as relative to DefaultSaveDirectory rather
    // than the current directory.
    private static string ResolvePath(string path)
    {
        var withExtension = Path.HasExtension(path) ? path : path + ".json";
        return Path.GetDirectoryName(withExtension) is { Length: > 0 }
            ? withExtension
            : Path.Combine(DefaultSaveDirectory, withExtension);
    }
}
