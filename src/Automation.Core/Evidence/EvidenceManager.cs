using System.Text.Json;
using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Automation.Core.Evidence;

public sealed class EvidenceManager
{
    private readonly ILogger _logger;

    public EvidenceManager(ILogger logger) => _logger = logger;

    public string CreateScenarioFolder(string runId, string featureName, string scenarioName)
    {
        var safeFeature = Sanitize(featureName);
        var safeScenario = Sanitize(scenarioName);
        var dir = Path.Combine("Artifacts", runId, safeFeature, safeScenario);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void CaptureFailureArtifacts(IWebDriver driver, string folder, object metadata)
    {
        try
        {
            var pngPath = Path.Combine(folder, "screenshot.png");
            var htmlPath = Path.Combine(folder, "page.html");
            var metaPath = Path.Combine(folder, "metadata.json");

            if (driver is ITakesScreenshot ss)
            {
                var shot = ss.GetScreenshot();
                shot.SaveAsFile(pngPath);
            }

            File.WriteAllText(htmlPath, driver.PageSource);
            File.WriteAllText(metaPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            _logger.LogInformation("Evidence captured at {Folder}", folder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture evidence.");
        }
    }

    private static string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s.Replace(' ', '_');
    }
}
