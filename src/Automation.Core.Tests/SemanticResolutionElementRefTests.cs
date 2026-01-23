using Xunit;

namespace Automation.Core.Tests
{
    public class SemanticResolutionElementRefTests
    {
        [Fact]
        public void Resolves_To_ElementOnly_When_ElementKey_Is_Unique()
        {
            // Build a minimal UiMap with two pages. Only the 'quote' page defines 'client-name'.
            var uiMap = new Automation.Core.UiMap.UiMapModel();
            uiMap.Pages["quote"] = new System.Collections.Generic.Dictionary<string, object>
            {
                ["__meta"] = new System.Collections.Generic.Dictionary<string, object> { ["route"] = "/app.html#/quote" },
                ["client-name"] = new System.Collections.Generic.Dictionary<string, object> { ["testId"] = "quote.client.name" }
            };
            uiMap.Pages["other"] = new System.Collections.Generic.Dictionary<string, object>
            {
                ["__meta"] = new System.Collections.Generic.Dictionary<string, object> { ["route"] = "/other" }
            };

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"quote.client-name\"\n";
            var metadata = new Automation.Core.Recorder.Draft.DraftMetadata();
            // Simulate mapping: one mapping for step line 7
            metadata.Mappings.Add(new Automation.Core.Recorder.Draft.DraftMapping { DraftLine = 7, EventIndex = -1, ActionIndex = 0, Confidence = 1.0 });

            var resolver = new Automation.Core.Recorder.Semantic.SemanticResolver(uiMap, null, 5, "draft.feature", "uimap.yaml");
            var (meta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Contains("Quando eu clico em \"client-name\"", resolvedFeature);
        }
    }
}
