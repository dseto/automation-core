using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Automation.Core.Tests
{
    public class SemanticE2ETests
    {
        [Fact]
        public void EndToEnd_SemanticResolution_Passes_Validation()
        {
            var temp = Path.Combine(Path.GetTempPath(), "semres-e2e-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);

            var root = Directory.GetCurrentDirectory();

            // 1) generate-draft
            var gen = RunDotnet($"run --project src/Automation.RecorderTool generate-draft --input specs/api/examples/recorder.session.login.example.json --output {temp}");
            Assert.True(gen.ExitCode == 0, $"generate-draft failed. STDOUT:\n{gen.StdOut}\nSTDERR:\n{gen.StdErr}");
            var draft = Path.Combine(temp, "draft.feature");
            var draftMeta = Path.Combine(temp, "draft.metadata.json");
            Assert.True(File.Exists(draft));
            Assert.True(File.Exists(draftMeta));

            // 2) resolve-draft
            var resolvedOut = Path.Combine(temp, "resolved");
            var res = RunDotnet($"run --project src/Automation.RecorderTool resolve-draft --draft \"{draft}\" --metadata \"{draftMeta}\" --ui-map specs/api/examples/uimap.example.yaml --output \"{resolvedOut}\"");
            Assert.True(res.ExitCode == 0, $"resolve-draft failed. STDOUT:\n{res.StdOut}\nSTDERR:\n{res.StdErr}");

            var resolvedMeta = Path.Combine(resolvedOut, "resolved.metadata.json");
            var uiGaps = Path.Combine(resolvedOut, "ui-gaps.report.json");
            Assert.True(File.Exists(resolvedMeta));
            Assert.True(File.Exists(uiGaps));

            // 3) validate with automation-validator
            var val = RunDotnet($"run --project src/Automation.Validator validate --resolved \"{resolvedMeta}\" --ui-gaps \"{uiGaps}\"");
            Assert.True(val.ExitCode == 0, $"validator failed. STDOUT:\n{val.StdOut}\nSTDERR:\n{val.StdErr}");

            Directory.Delete(temp, true);
        }

        [Fact]
        public void GenerateDraft_UsesScenarioNameParameter()
        {
            var temp = Path.Combine(Path.GetTempPath(), "semres-e2e-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);

            var scenario = "cenariosegurotest";
            var gen = RunDotnet($"run --project src/Automation.RecorderTool generate-draft --input specs/api/examples/recorder.session.login.example.json --output {temp} --scenario \"{scenario}\"");
            Assert.True(gen.ExitCode == 0, $"generate-draft failed. STDOUT:\n{gen.StdOut}\nSTDERR:\n{gen.StdErr}");

            var draft = Path.Combine(temp, "draft.feature");
            Assert.True(File.Exists(draft));
            var content = File.ReadAllText(draft);
            Assert.Contains($"Cenário: {scenario}", content);

            Directory.Delete(temp, true);
        }

        private (int ExitCode, string StdOut, string StdErr) RunDotnet(string args)
        {
            var psi = new ProcessStartInfo("dotnet", args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            // Ensure we run from repo root (where the solution file is)
            var dir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(dir, "AutomationPlatform.sln")))
            {
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            psi.WorkingDirectory = dir;

            using var proc = Process.Start(psi)!;
            var outp = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            return (proc.ExitCode, outp, err);
        }
    }
}
