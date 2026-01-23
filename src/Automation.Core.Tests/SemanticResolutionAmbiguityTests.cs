using Xunit;
using System.Linq;

namespace Automation.Core.Tests
{
    public class SemanticResolutionAmbiguityTests
    {
        [Fact]
        public void AmbiguousElementKey_EmitsWarning_But_UsesElementOnly()
        {
            var uiMap = new Automation.Core.UiMap.UiMapModel();
            uiMap.Pages["quote"] = new System.Collections.Generic.Dictionary<string, object>
            {
                ["__meta"] = new System.Collections.Generic.Dictionary<string, object> { ["route"] = "/app.html#/quote" },
                ["client-name"] = new System.Collections.Generic.Dictionary<string, object> { ["testId"] = "quote.client.name" }
            };
            uiMap.Pages["other"] = new System.Collections.Generic.Dictionary<string, object>
            {
                ["__meta"] = new System.Collections.Generic.Dictionary<string, object> { ["route"] = "/other" },
                ["client-name"] = new System.Collections.Generic.Dictionary<string, object> { ["testId"] = "other.client.name" }
            };

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"quote.client-name\"\n";
            var metadata = new Automation.Core.Recorder.Draft.DraftMetadata();
            metadata.Mappings.Add(new Automation.Core.Recorder.Draft.DraftMapping { DraftLine = 7, EventIndex = -1, ActionIndex = 0, Confidence = 1.0 });

            var resolver = new Automation.Core.Recorder.Semantic.SemanticResolver(uiMap, null, 5, "draft.feature", "uimap.yaml");
            var (meta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Contains("Quando eu clico em \"client-name\"", resolvedFeature);
            Assert.Contains(report.Findings, f => f.Code == "UIGAP_ELEMENT_AMBIGUOUS" && f.DraftLine == 7);
        }
    }
}
