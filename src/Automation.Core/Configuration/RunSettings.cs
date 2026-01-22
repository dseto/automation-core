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
        ) { RecordWaitLogThresholdSeconds = GetDouble("RECORD_WAIT_LOG_THRESHOLD_SECONDS", 1.0) };

        return settings;
    }
}
