using System.IO;
using System.Text.Json;

namespace Automation.Core.Recorder;

public sealed class SessionWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string Write(RecorderSession session, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var path = Path.Combine(outputDir, "session.json");
        var json = JsonSerializer.Serialize(session, JsonOptions);
        File.WriteAllText(path, json);

        return path;
    }
}
