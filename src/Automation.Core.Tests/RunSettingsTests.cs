using System;
using Automation.Core.Configuration;
using Xunit;

namespace Automation.Core.Tests
{
    public class RunSettingsTests : IDisposable
    {
        private readonly string? _origUiMap;
        private readonly string? _origUiMapAlias;
        private readonly string? _origSemOut;
        private readonly string? _origMaxCandidates;
        private readonly string? _origConfResolved;
        private readonly string? _origConfPartial;

        public RunSettingsTests()
        {
            _origUiMap = Environment.GetEnvironmentVariable("UI_MAP_PATH");
            _origUiMapAlias = Environment.GetEnvironmentVariable("UIMAP_PATH");
            _origSemOut = Environment.GetEnvironmentVariable("SEMRES_OUTPUT_DIR");
            _origMaxCandidates = Environment.GetEnvironmentVariable("SEMRES_MAX_CANDIDATES");
            _origConfResolved = Environment.GetEnvironmentVariable("SEMRES_CONFIDENCE_RESOLVED_MIN");
            _origConfPartial = Environment.GetEnvironmentVariable("SEMRES_CONFIDENCE_PARTIAL_MIN");
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("UI_MAP_PATH", _origUiMap);
            Environment.SetEnvironmentVariable("UIMAP_PATH", _origUiMapAlias);
            Environment.SetEnvironmentVariable("SEMRES_OUTPUT_DIR", _origSemOut);
            Environment.SetEnvironmentVariable("SEMRES_MAX_CANDIDATES", _origMaxCandidates);
            Environment.SetEnvironmentVariable("SEMRES_CONFIDENCE_RESOLVED_MIN", _origConfResolved);
            Environment.SetEnvironmentVariable("SEMRES_CONFIDENCE_PARTIAL_MIN", _origConfPartial);
        }

        [Fact]
        public void UiMapPath_Prefers_UI_MAP_PATH_over_alias()
        {
            Environment.SetEnvironmentVariable("UI_MAP_PATH", "ui/canonical.yaml");
            Environment.SetEnvironmentVariable("UIMAP_PATH", "ui/alias.yaml");

            var settings = RunSettings.FromEnvironment();
            Assert.Equal("ui/canonical.yaml", settings.UiMapPath);
        }

        [Fact]
        public void UiMapPath_Uses_alias_when_UI_MAP_PATH_not_set()
        {
            Environment.SetEnvironmentVariable("UI_MAP_PATH", null);
            Environment.SetEnvironmentVariable("UIMAP_PATH", "ui/alias2.yaml");

            var settings = RunSettings.FromEnvironment();
            Assert.Equal("ui/alias2.yaml", settings.UiMapPath);
        }

        [Fact]
        public void UiMapPath_Defaults_when_none_set()
        {
            Environment.SetEnvironmentVariable("UI_MAP_PATH", null);
            Environment.SetEnvironmentVariable("UIMAP_PATH", null);

            var settings = RunSettings.FromEnvironment();
            Assert.Equal("specs/frontend/uimap.yaml", settings.UiMapPath);
        }

        [Fact]
        public void SemRes_Settings_Are_Parsed_FromEnv()
        {
            Environment.SetEnvironmentVariable("SEMRES_OUTPUT_DIR", "out/semres");
            Environment.SetEnvironmentVariable("SEMRES_MAX_CANDIDATES", "7");
            Environment.SetEnvironmentVariable("SEMRES_CONFIDENCE_RESOLVED_MIN", "0.9");
            Environment.SetEnvironmentVariable("SEMRES_CONFIDENCE_PARTIAL_MIN", "0.55");

            var settings = RunSettings.FromEnvironment();
            Assert.Equal("out/semres", settings.SemResOutputDir);
            Assert.Equal(7, settings.SemResMaxCandidates);
            Assert.Equal(0.9, settings.SemResConfidenceResolvedMin);
            Assert.Equal(0.55, settings.SemResConfidencePartialMin);
        }
    }
}
