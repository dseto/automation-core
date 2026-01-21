using System.IO;
using System.Text.Json;

namespace Automation.Core.Recorder;

public sealed class SessionReader
{
    public RecorderSession Read(string path)
    {
        var json = File.ReadAllText(path);
        var session = JsonSerializer.Deserialize<RecorderSession>(json);
        if (session == null)
            throw new InvalidDataException("Invalid session.json content.");

        return session;
    }
}
