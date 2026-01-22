using System.IO;
using Automation.Core.Recorder.Semantic;
using Automation.Core.Recorder.Semantic.Models;
using Xunit;

namespace Automation.Core.Tests
{
    public class SemanticWriterTests
    {
        [Fact]
        public void Writes_Resolved_Artifacts()
        {
            var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var writer = new SemanticWriter();

            var featurePath = writer.WriteResolvedFeature("#language: pt\n\nFuncionalidade: X\n", temp);
            Assert.True(File.Exists(featurePath));

            var meta = new ResolvedMetadata { Version = "1.0", GeneratedAt = "now", Source = new ResolvedSource { DraftFeaturePath = "draft.feature", UiMapPath = "ui.yaml", SessionPath = null } };
            var metaPath = writer.WriteResolvedMetadata(meta, temp);
            Assert.True(File.Exists(metaPath));

            var report = new UiGapsReport { SessionId = "S1", GeneratedAt = "now", DraftPath = "draft.feature", UiMapPath = "ui.yaml" };
            report.Stats = new UiGapStats { Errors = 0, Warnings = 0, Infos = 0, Total = 0 };
            var jsonPath = writer.WriteUiGapsReport(report, temp);
            Assert.True(File.Exists(jsonPath));

            // Cleanup
            Directory.Delete(temp, true);
        }
    }
}
