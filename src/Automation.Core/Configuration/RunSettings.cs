using System;
using System.Globalization;

namespace Automation.Core.Configuration;

public sealed record RunSettings(
    string BaseUrl,
    string Browser,
    bool Headless,
    bool UiDebug,
    int SlowMoMs,
    bool Highlight,
    bool PauseOnFailure,
    bool WaitAngular,
    int AngularTimeoutMs,
    int StepTimeoutMs,
    string EnvironmentName,
    bool RecordEnabled,
    string RecordOutputDir)
{
    public double RecordWaitLogThresholdSeconds { get; init; } = 1.0;

    // Semantic Resolution settings (additive; init-only to avoid breaking constructor)
    public string UiMapPath { get; init; } = "specs/frontend/uimap.yaml";
    public string SemResOutputDir { get; init; } = "artifacts/semantic-resolution";
    public int SemResMaxCandidates { get; init; } = 5;
    public double SemResConfidenceResolvedMin { get; init; } = 0.85;
    public double SemResConfidencePartialMin { get; init; } = 0.60;

    public static RunSettings FromEnvironment()
    {
        static string Get(string key, string def) => Environment.GetEnvironmentVariable(key) ?? def;

        static bool GetBool(string key, bool def)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return v is null ? def : v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1";
        }

        static int GetInt(string key, int def)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return int.TryParse(v, out var i) ? i : def;
        }

        static double GetDouble(string key, double def)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : def;
        }

        var uiDebug = GetBool("UI_DEBUG", false);
        var isCi = GetBool("CI", false) || GetBool("TF_BUILD", false) || GetBool("GITHUB_ACTIONS", false) || GetBool("BUILD_BUILDID", false);
        if (isCi && uiDebug) uiDebug = false; // debug visual is local-only
        var headless = GetBool("HEADLESS", true);
        if (uiDebug) headless = false; // debug requires headed
        var browser = Get("BROWSER", "edge");

        // Resolve UiMap path with precedence: UI_MAP_PATH (canonical) > UIMAP_PATH (alias) > default
        var uiMapPath = Environment.GetEnvironmentVariable("UI_MAP_PATH");
        if (string.IsNullOrWhiteSpace(uiMapPath)) uiMapPath = Environment.GetEnvironmentVariable("UIMAP_PATH");
        if (string.IsNullOrWhiteSpace(uiMapPath)) uiMapPath = "specs/frontend/uimap.yaml";

        var settings = new RunSettings(
            BaseUrl: Get("BASE_URL", ""),
            Browser: browser,
            Headless: headless,
            UiDebug: uiDebug,
            SlowMoMs: GetInt("SLOWMO_MS", uiDebug ? 250 : 0),
            Highlight: GetBool("HIGHLIGHT", uiDebug),
            PauseOnFailure: GetBool("PAUSE_ON_FAILURE", uiDebug),
            WaitAngular: GetBool("WAIT_ANGULAR", true),
            AngularTimeoutMs: GetInt("ANGULAR_TIMEOUT_MS", 5000),
            StepTimeoutMs: GetInt("STEP_TIMEOUT_MS", 20000),
            EnvironmentName: Get("TEST_ENV", "default"),
            RecordEnabled: GetBool("AUTOMATION_RECORD", false),
            RecordOutputDir: Get("RECORD_OUTPUT_DIR", "artifacts/recorder")
        )
        {
            RecordWaitLogThresholdSeconds = GetDouble("RECORD_WAIT_LOG_THRESHOLD_SECONDS", 1.0),
            UiMapPath = uiMapPath,
            SemResOutputDir = Get("SEMRES_OUTPUT_DIR", "artifacts/semantic-resolution"),
            SemResMaxCandidates = GetInt("SEMRES_MAX_CANDIDATES", 5),
            SemResConfidenceResolvedMin = GetDouble("SEMRES_CONFIDENCE_RESOLVED_MIN", 0.85),
            SemResConfidencePartialMin = GetDouble("SEMRES_CONFIDENCE_PARTIAL_MIN", 0.60)
        };

        return settings;
    }
}
