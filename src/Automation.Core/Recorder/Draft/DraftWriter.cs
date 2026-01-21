using System.IO;
using System.Text.Json;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string WriteMetadata(DraftMetadata metadata, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var path = Path.Combine(outputDir, "draft.metadata.json");
        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        File.WriteAllText(path, json);
        return path;
    }

    public string WriteFeature(string content, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var path = Path.Combine(outputDir, "draft.feature");
        File.WriteAllText(path, content);
        return path;
    }
}
