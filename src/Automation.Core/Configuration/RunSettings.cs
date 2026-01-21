using System;

namespace Automation.Core.Configuration;

public sealed record RunSettings(
    string BaseUrl,
    bool Headless,
    bool UiDebug,
    int SlowMoMs,
    bool Highlight,
    bool PauseOnFailure,
    bool WaitAngular,
    int AngularTimeoutMs,
    int StepTimeoutMs,
    string EnvironmentName)
{
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

        var uiDebug = GetBool("UI_DEBUG", false);
        var isCi = GetBool("CI", false) || GetBool("TF_BUILD", false) || GetBool("GITHUB_ACTIONS", false) || GetBool("BUILD_BUILDID", false);
        if (isCi && uiDebug) uiDebug = false; // debug visual is local-only
        var headless = GetBool("HEADLESS", true);
        if (uiDebug) headless = false; // debug requires headed

        return new RunSettings(
            BaseUrl: Get("BASE_URL", ""),
            Headless: headless,
            UiDebug: uiDebug,
            SlowMoMs: GetInt("SLOWMO_MS", uiDebug ? 250 : 0),
            Highlight: GetBool("HIGHLIGHT", uiDebug),
            PauseOnFailure: GetBool("PAUSE_ON_FAILURE", uiDebug),
            WaitAngular: GetBool("WAIT_ANGULAR", true),
            AngularTimeoutMs: GetInt("ANGULAR_TIMEOUT_MS", 5000),
            StepTimeoutMs: GetInt("STEP_TIMEOUT_MS", 20000),
            EnvironmentName: Get("TEST_ENV", "default")
        );
    }
}
