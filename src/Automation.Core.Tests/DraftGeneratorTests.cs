using System.IO;
using Xunit;

namespace Automation.Core.Tests
{
    public class DraftGeneratorTests
    {
        [Fact]
        public void Waits_RoundedToInteger_And_omit_less_than_1s()
        {
            // Build a minimal session with an event that has waitMs 1200
            var session = new Automation.Core.Recorder.RecorderSession();
            session.SessionId = "S-test";
            session.StartedAt = System.DateTimeOffset.Now;
            session.EndedAt = System.DateTimeOffset.Now;
            session.Events = new System.Collections.Generic.List<Automation.Core.Recorder.RecorderEvent>();

            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:00.000", Type = "navigate", Route = "/" });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:01.200", Type = "click", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "div" }, WaitMs = 1200 });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:02.000", Type = "click", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "div" } , WaitMs = 900});

            // ensure WaitMs was set on the raw events
            Assert.True(session.Events[1].WaitMs.HasValue, $"WaitMs is null. Event: {System.Text.Json.JsonSerializer.Serialize(session.Events[1])}");

            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            var gen = new Automation.Core.Recorder.Draft.DraftGenerator(
                new Automation.Core.Recorder.Draft.SessionSanityChecker(),
                new Automation.Core.Recorder.Draft.ActionGrouper(),
                new Automation.Core.Recorder.Draft.StepInferenceEngine(),
                new Automation.Core.Recorder.Draft.EscapeHatchRenderer(),
                new Automation.Core.Recorder.Draft.DraftWriter());

            // Sanity-check the grouping behavior
            var actions = gen.Grouper.Group(session);
            Assert.NotEmpty(actions);
            var clickAction = System.Linq.Enumerable.First(actions, a => a.EventIndexes.Contains(1));
            var firstPrimaryWait = clickAction.PrimaryEvent?.WaitMs;
            Assert.Equal(1200, firstPrimaryWait);

            var result = gen.Generate(session, dir);
            if (!result.IsSuccess)
            {
                var metadataPath = result.MetadataPath ?? System.IO.Path.Combine(dir, "draft.metadata.json");
                var metadata = System.IO.File.Exists(metadataPath) ? System.IO.File.ReadAllText(metadataPath) : "(no metadata)";
                Assert.True(result.IsSuccess, $"Generation failed. InputStatus={result.InputStatus}. Metadata:\n{metadata}");
            }

            var draft = File.ReadAllText(Path.Combine(dir, "draft.feature"));
            Assert.True(draft.Contains("E eu espero 1 segundos"), $"Draft doesn't contain expected wait. Draft:\n{draft}");
            // 900ms wait should be omitted
            Assert.DoesNotContain("E eu espero 0 segundos", draft);

            Directory.Delete(dir, true);
        }

        [Fact]
        public void Generates_steps_for_elements_with_data_testid()
        {
            var session = new Automation.Core.Recorder.RecorderSession();
            session.SessionId = "S-test";
            session.StartedAt = System.DateTimeOffset.Now;
            session.EndedAt = System.DateTimeOffset.Now;
            session.Events = new System.Collections.Generic.List<Automation.Core.Recorder.RecorderEvent>();

            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:00.000", Type = "navigate", Route = "/" });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:01.000", Type = "click", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "[data-testid='page.login.username']", ["attributes"] = new System.Collections.Generic.Dictionary<string, object?> { ["data-testid"] = "page.login.username" } } });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:02.000", Type = "fill", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "[data-testid='page.login.username']", ["attributes"] = new System.Collections.Generic.Dictionary<string, object?> { ["data-testid"] = "page.login.username" } }, Value = new System.Collections.Generic.Dictionary<string, object?> { ["literal"] = "fasdfsd" } });

            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            var gen = new Automation.Core.Recorder.Draft.DraftGenerator(
                new Automation.Core.Recorder.Draft.SessionSanityChecker(),
                new Automation.Core.Recorder.Draft.ActionGrouper(),
                new Automation.Core.Recorder.Draft.StepInferenceEngine(),
                new Automation.Core.Recorder.Draft.EscapeHatchRenderer(),
                new Automation.Core.Recorder.Draft.DraftWriter());

            var result = gen.Generate(session, dir);
            Assert.True(result.IsSuccess, $"Generation failed: {result.Warning}");

            var draft = File.ReadAllText(Path.Combine(dir, "draft.feature"));
            // The click may be merged with the subsequent fill into a single action; ensure at least the fill step is generated.
            Assert.Contains("Quando eu preencho \"page.login.username\" com \"fasdfsd\"", draft);

            Directory.Delete(dir, true);
        }

        [Fact]
        public void ActionGrouper_Merges_GenericClickAndDataTestIdFill()
        {
            var session = new Automation.Core.Recorder.RecorderSession();
            session.SessionId = "S-merge";
            session.StartedAt = System.DateTimeOffset.Now;
            session.EndedAt = System.DateTimeOffset.Now;
            session.Events = new System.Collections.Generic.List<Automation.Core.Recorder.RecorderEvent>();

            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:00.000", Type = "navigate", Route = "/" });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:01.000", Type = "click", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "div" } });
            session.Events.Add(new Automation.Core.Recorder.RecorderEvent { T = "00:01.500", Type = "fill", Target = new System.Collections.Generic.Dictionary<string, object?> { ["hint"] = "[data-testid='page.login.username']", ["attributes"] = new System.Collections.Generic.Dictionary<string, object?> { ["data-testid"] = "page.login.username" } }, Value = new System.Collections.Generic.Dictionary<string, object?> { ["literal"] = "x" } });

            var grouper = new Automation.Core.Recorder.Draft.ActionGrouper();
            var actions = grouper.Group(session);

            // click + fill should be merged into a single grouped action (navigate stays separate)
            Assert.Equal(2, System.Linq.Enumerable.Count(actions));
            Assert.True(System.Linq.Enumerable.Any(actions, a => a.EventIndexes.Contains(1) && a.EventIndexes.Contains(2)));
        }
    }
}