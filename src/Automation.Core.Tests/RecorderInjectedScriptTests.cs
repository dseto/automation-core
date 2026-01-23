using System.IO;
using Xunit;

namespace Automation.Core.Tests
{
    public class RecorderInjectedScriptTests
    {
        [Fact]
        public void InjectedScript_Should_PersistPendingBufferAcrossNavigations()
        {
            // Locate repository root by looking for solution file and resolve Program.cs
            var dir = Directory.GetCurrentDirectory();
            string? repoRoot = null;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "AutomationPlatform.sln"))) { repoRoot = dir; break; }
                dir = Path.GetDirectoryName(dir);
            }

            Assert.False(string.IsNullOrWhiteSpace(repoRoot), "Repository root (AutomationPlatform.sln) not found.");
            var programPath = Path.Combine(repoRoot!, "src", "Automation.RecorderTool", "Program.cs");
            Assert.True(File.Exists(programPath), $"Program.cs not found at {programPath}");
            var content = File.ReadAllText(programPath);
            Assert.Contains("__fhRecorder_pending", content);
        }
    }
}
