using System.Diagnostics;
using Xunit;

namespace Automation.Core.Tests
{
    public class SemResToggleTests
    {
        [Fact]
        public void ResolveDraft_Respects_SemResEnabled_Flag()
        {
            // compute path to built RecorderTool dll (assumes build has run)
            // Find repository root by locating the solution file
            var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            string repoRoot = null!;
            while (dir != null)
            {
                var candidate = System.IO.Path.Combine(dir.FullName, "AutomationPlatform.sln");
                if (System.IO.File.Exists(candidate)) { repoRoot = dir.FullName; break; }
                dir = dir.Parent;
            }
            Assert.False(string.IsNullOrWhiteSpace(repoRoot), "Could not locate repository root (AutomationPlatform.sln)");

            var recorderProj = System.IO.Path.Combine(repoRoot, "src", "Automation.RecorderTool", "Automation.RecorderTool.csproj");
            Assert.True(System.IO.File.Exists(recorderProj), $"RecorderTool project not found at {recorderProj}");

            var psi = new ProcessStartInfo("dotnet")
            {
                Arguments = $"run --project \"{recorderProj}\" resolve-draft --draft dummy --metadata dummy",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = repoRoot
            };

            // Set SEMRES_ENABLED to false to disable semantic resolution
            psi.Environment["SEMRES_ENABLED"] = "false";

            using var p = Process.Start(psi)!;
            Assert.NotNull(p);

            // Wait up to 10s for process to exit
            var exited = p.WaitForExit(10000);
            if (!exited)
            {
                p.Kill(true);
                Assert.False(true, "Process did not exit in time");
            }

            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();

            // Expect non-zero exit code and message about SEMRES_ENABLED
            Assert.NotEqual(0, p.ExitCode);
            Assert.Contains("SEMRES_ENABLED", stdout + stderr);
            Assert.Contains("Semantic resolution disabled", stdout + stderr);
        }
    }
}
