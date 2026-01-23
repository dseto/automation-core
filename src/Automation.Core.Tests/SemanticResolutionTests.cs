using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation.Core.Recorder;
using Automation.Core.Recorder.Draft;
using Automation.Core.Recorder.Semantic;
using Automation.Core.Recorder.Semantic.Models;
using Automation.Core.UiMap;
using Xunit;

namespace Automation.Core.Tests
{
    public class SemanticResolutionTests
    {
        private UiMapModel BuildUiMap(IEnumerable<(string page, string element, string testId)> items)
        {
            var ui = new UiMapModel();
            foreach (var grp in items.GroupBy(i => i.page))
            {
                var pageDict = new Dictionary<string, object>();
                var pageName = grp.Key;
                foreach (var e in grp)
                {
                    var elementDict = new Dictionary<string, object>
                    {
                        ["testId"] = e.testId
                    };
                    pageDict[e.element] = elementDict;
                }
                ui.Pages[pageName] = pageDict;
            }
            return ui;
        }

        private DraftMetadata BuildDraftMetadata(params (int eventIndex, int draftLine)[] entries)
        {
            var dm = new DraftMetadata
            {
                SessionId = "S1",
                EventsCount = 0,
                ActionsCount = 0,
                StepsInferredCount = entries.Length,
                EscapeHatchCount = 0,
                Warnings = new List<string>(),
                Mappings = entries.Select(e => new DraftMapping { EventIndex = e.eventIndex, DraftLine = e.draftLine, Confidence = 1.0 }).ToList()
            };
            return dm;
        }

        private RecorderSession BuildSessionWithTestIds(params string[] testIds)
        {
            var session = new RecorderSession();
            for (int i = 0; i < testIds.Length; i++)
            {
                var ev = new RecorderEvent
                {
                    Type = "click",
                    Target = new Dictionary<string, object>
                    {
                        ["attributes"] = new Dictionary<string, object>
                        {
                            ["data-testid"] = testIds[i]
                        }
                    }
                };
                session.Events.Add(ev);
            }
            return session;
        }

        private RecorderSession BuildSessionWithHints(params string[] hints)
        {
            var session = new RecorderSession();
            for (int i = 0; i < hints.Length; i++)
            {
                var ev = new RecorderEvent
                {
                    Type = "click",
                    Target = new Dictionary<string, object>
                    {
                        ["hint"] = hints[i]
                    }
                };
                session.Events.Add(ev);
            }
            return session;
        }

        [Fact]
        public void Exact_UIMap_Key_Matches_As_Resolved_With_Confidence_One()
        {
            var ui = BuildUiMap(new[] { ("login", "username", "login.username") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"login.username\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            // mapping refers to line 7 where the step is

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Single(resolvedMeta.Steps);
            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("login", s.Chosen!.PageKey);
            Assert.Equal("username", s.Chosen!.ElementKey);
            Assert.Equal("login.username", s.Chosen!.TestId);
            Assert.Empty(report.Findings.Where(f => f.DraftLine == 7));
        }

        [Fact]
        public void Unique_TestId_From_Session_Resolves_With_Confidence_One()
        {
            var ui = BuildUiMap(new[] { ("home", "btnLogin", "btn-login") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"something\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithTestIds("btn-login");

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("home", s.Chosen!.PageKey);
            Assert.Equal("btnLogin", s.Chosen!.ElementKey);
            Assert.Equal("btn-login", s.Chosen!.TestId);
            Assert.Empty(report.Findings.Where(f => f.DraftLine == 7));
        }

        [Fact]
        public void Multiple_Candidates_Produces_Partial_With_Confidence_OneOverN_And_Ambiguity_Finding()
        {
            var ui = BuildUiMap(new[] { ("a", "x", "dup"), ("b", "y", "dup") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"something\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithTestIds("dup");

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("partial", s.Status);
            Assert.Equal(2, s.Candidates.Count);

            var warn = report.Findings.FirstOrDefault(f => f.DraftLine == 7 && f.Code == "AMBIGUOUS_MATCH");
            Assert.NotNull(warn);
            Assert.Equal("warn", warn.Severity);

            // metadata should reference finding codes
            Assert.NotEmpty(s.Findings);
            Assert.Contains(s.Findings, ff => ff.Code == "AMBIGUOUS_MATCH");
            Assert.All(s.Findings.Select(ff => ff.Code), code => Assert.Contains(code, report.Findings.Select(f => f.Code)));
        }

        [Fact]
        public void Zero_Candidates_Marks_Unresolved_With_Error_Finding()
        {
            var ui = BuildUiMap(new[] { ("a", "x", "id1") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"something\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithTestIds("unknown-id");

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("unresolved", s.Status);

            var err = report.Findings.FirstOrDefault(f => f.DraftLine == 7 && f.Code == "TESTID_NOT_FOUND_IN_UIMAP");
            Assert.NotNull(err);
            Assert.Equal("error", err.Severity);

            Assert.NotEmpty(s.Findings);
            Assert.Contains(s.Findings, ff => ff.Code == "TESTID_NOT_FOUND_IN_UIMAP");
            Assert.All(s.Findings.Select(ff => ff.Code), code => Assert.Contains(code, report.Findings.Select(f => f.Code)));
        }

        [Fact]
        public void Candidate_Truncation_Adds_Info_Finding_And_limits_candidates()
        {
            var ui = BuildUiMap(new[] { ("p1", "a", "c"), ("p2", "b", "c"), ("p3", "c", "c") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"x\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithTestIds("c");

            var resolver = new SemanticResolver(ui, session, 2, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("partial", s.Status);
            Assert.Equal(2, s.Candidates.Count);
            var info = report.Findings.FirstOrDefault(f => f.DraftLine == 7 && f.Code == "CANDIDATES_TRUNCATED");
            Assert.NotNull(info);
            Assert.Equal("info", info.Severity);
        }

        [Fact]
        public void UiGapsIds_Are_Deterministic_And_Sequential()
        {
            var ui = BuildUiMap(new[] { ("a", "x", "id1") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"something\"\n  Quando eu clico em \"something\"\n";
            var metadata = BuildDraftMetadata((0, 7), (1, 8));
            var session = BuildSessionWithTestIds("unknown", "unknown");

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Equal(report.Findings.Count, report.Stats.Total);
            Assert.All(report.Findings.Select((f, i) => new { f, i }), x => Assert.Equal($"UIGAP-{(x.i + 1).ToString("D4")}", x.f.Id));

            // Ensure ordering: draftLine asc
            Assert.True(report.Findings.SequenceEqual(report.Findings.OrderBy(f => f.DraftLine).ThenBy(f => f.Severity).ThenBy(f => f.Code)));
        }

        [Fact]
        public void Hint_with_data_testid_parses_and_resolves()
        {
            var ui = BuildUiMap(new[] { ("login", "username", "page.login.username") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu preencho \"[data-testid='page.login.username']\" com \"x\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithHints("[data-testid='page.login.username']");
            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Single(resolvedMeta.Steps);
            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("login", s.Chosen!.PageKey);
            Assert.Equal("username", s.Chosen!.ElementKey);
            Assert.Equal("page.login.username", s.Chosen!.TestId);
            Assert.Empty(report.Findings.Where(f => f.DraftLine == 7));
        }

        [Fact]
        public void Resolved_Fill_Preserves_Literal_When_Element_Rewritten()
        {
            var ui = BuildUiMap(new[] { ("login", "username", "page.login.username") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu preencho \"[data-testid='page.login.username']\" com \"admin\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithHints("[data-testid='page.login.username']");
            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Single(resolvedMeta.Steps);
            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("login", s.Chosen!.PageKey);
            Assert.Equal("username", s.Chosen!.ElementKey);
            Assert.Equal("page.login.username", s.Chosen!.TestId);

            // literal should be preserved verbatim
            Assert.Contains("com \"admin\"", resolvedFeature);
            Assert.Contains("Quando eu preencho \"login.username\" com \"admin\"", resolvedFeature);
            Assert.DoesNotContain("[data-testid='page.login.username']", resolvedFeature);
        }

        [Fact]
        public void Only_First_Quoted_String_Is_Rewritten()
        {
            var ui = BuildUiMap(new[] { ("login", "username", "page.login.username") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu preencho \"[data-testid='page.login.username']\" com \"x\" e depois digo \"y\"\n";
            var metadata = BuildDraftMetadata((0, 7));
            var session = BuildSessionWithHints("[data-testid='page.login.username']");
            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Single(resolvedMeta.Steps);
            // only the first quoted string (the reference) should be rewritten; subsequent quoted strings must remain as-is
            Assert.Contains("Quando eu preencho \"login.username\" com \"x\" e depois digo \"y\"", resolvedFeature);
        }

        [Fact]
        public void Navigate_Unmapped_Route_Produces_Info_Finding()
        {
            var ui = BuildUiMap(new[] { ("home", "btnLogin", "btn-login") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"unknown-route\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("unresolved", s.Status);

            var info = report.Findings.FirstOrDefault(f => f.DraftLine == 7 && f.Code == "UIGAP_ROUTE_NOT_MAPPED");
            Assert.NotNull(info);
            Assert.Equal("info", info.Severity);
            Assert.Equal(1, report.Stats.Infos);
        }

        [Fact]
        public void Navigate_Mapped_Route_Resolves_To_Page()
        {
            // Build a UiMap with a page that has __meta.route = "/dashboard"
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/dashboard"
                }
            };
            ui.Pages["dashboard"] = pageDict;

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"/dashboard\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            // For page navigation we resolve to the page and use a synthetic element key to indicate page-level resolution
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("dashboard", s.Chosen!.PageKey);
            Assert.Equal("__page__", s.Chosen!.ElementKey);
        }

        [Fact]
        public void Navigate_Mapped_Route_Rewrites_Step_To_PageKey()
        {
            // Build a UiMap with a page that has __meta.route = "/login.html"
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/login.html"
                }
            };
            ui.Pages["login"] = pageDict;

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"/login.html\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            // The resolved feature should contain the page key (login) instead of the literal route
            Assert.Contains("Dado que estou na página \"login\"", resolvedFeature);
            Assert.DoesNotContain("/login.html", resolvedFeature);
        }

        [Fact]
        public void Resolved_Feature_Includes_BaseUrl_Step_When_PageNavigations_Exist()
        {
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/dashboard"
                }
            };
            ui.Pages["dashboard"] = pageDict;

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"/dashboard\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            // Expect the resolved feature to include an explicit BASE_URL step at the start of the scenario
            Assert.Contains("Dado que a aplicação está em \"${BASE_URL}\"", resolvedFeature);
        }

        [Fact]
        public void Resolved_Feature_Rewrites_Element_Refs_To_ElementKey_When_Resolved()
        {
            // UiMap: element 'pass-label' exists with test_id 'login.pass.label'
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["pass-label"] = new Dictionary<string, object> { ["testId"] = "login.pass.label" }
            };
            ui.Pages["login"] = pageDict;

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"login.pass.label\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            // After resolution we expect the step to use the element key (pass-label) so runtime's ElementResolver can find it
            Assert.Contains("Quando eu clico em \"login.pass-label\"", resolvedFeature);
            Assert.DoesNotContain("login.pass.label", resolvedFeature);
        }

        [Fact]
        public void Navigate_Uses_Session_Event_Route_When_Present()
        {
            // UiMap has page with __meta.route = "/dashboard"
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/dashboard"
                }
            };
            ui.Pages["dashboard"] = pageDict;

            // Draft mentions an unknown route but session event carries the canonical route
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"unknown-route\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var session = new RecorderSession();
            session.Events.Add(new RecorderEvent { Type = "navigate", Route = "/dashboard" });

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("dashboard", s.Chosen!.PageKey);
            Assert.Equal("__page__", s.Chosen!.ElementKey);
        }

        [Fact]
        public void Treats_PageElement_WithDots_As_TestId_When_ElementNameMissing()
        {
            // Build UiMap where page 'layout' has element 'topbar-menu' with test_id = 'layout.topbar.menu'
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["topbar-menu"] = new Dictionary<string, object>
                {
                    ["testId"] = "layout.topbar.menu"
                }
            };
            ui.Pages["layout"] = pageDict;

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"layout.topbar.menu\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var resolver = new SemanticResolver(ui, null, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("layout", s.Chosen!.PageKey);
            Assert.Equal("topbar-menu", s.Chosen!.ElementKey);
            Assert.Equal("layout.topbar.menu", s.Chosen!.TestId);
        }

        [Fact]
        public void Navigate_Session_NoRoute_Produces_Info_Finding()
        {
            // UiMap with a page
            var ui = BuildUiMap(new[] { ("home", "btnLogin", "btn-login") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"some-route\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var session = new RecorderSession();
            session.Events.Add(new RecorderEvent { Type = "navigate" }); // no Route

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("unresolved", s.Status);

            var info = report.Findings.FirstOrDefault(f => f.DraftLine == 7 && f.Code == "UIGAP_ROUTE_NOT_MAPPED");
            Assert.NotNull(info);
            Assert.Equal("info", info.Severity);
        }

        [Theory]
        [InlineData("[data-testid='page.login.username']")]
        [InlineData("[data-testid=\"page.login.username\"]")]
        [InlineData("[data-testid = 'page.login.username']")]
        public void Recorder_Normalizes_Hint_Variants_And_Resolver_Resolves(string hint)
        {
            var ui = BuildUiMap(new[] { ("login", "username", "page.login.username") });
            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu preencho \"[data-testid='page.login.username']\" com \"x\"\n";
            var metadata = BuildDraftMetadata((0, 7));

            var rec = new SessionRecorder();
            rec.Start();
            rec.RecordClick(hint);
            var session = rec.GetSession();

            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            Assert.Single(resolvedMeta.Steps);
            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("page.login.username", s.Chosen!.TestId);
            Assert.Empty(report.Findings.Where(f => f.DraftLine == 7));
        }

        [Fact]
        public void SessionFile_hint_parses_and_resolves()
        {
            var sessionPath = Path.Combine("ui-tests", "artifacts", "recorder", "session.json");
            if (!File.Exists(sessionPath))
            {
                // If file not available, skip this test (helps local dev)
                return;
            }

            var sr = new SessionReader();
            var session = sr.Read(sessionPath);

            // find first event with hint containing page.login.username
            int idx = -1;
            for (int i = 0; i < session.Events.Count; i++)
            {
                var ev = session.Events[i];
                if (ev.Target is IDictionary<string, object> dict && dict.TryGetValue("hint", out var h) && h is string hs && hs.Contains("page.login.username")) { idx = i; break; }
                if (ev.Target is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (je.TryGetProperty("hint", out var hv) && hv.ValueKind == System.Text.Json.JsonValueKind.String && hv.GetString()?.Contains("page.login.username") == true) { idx = i; break; }
                }
            }

            Assert.True(idx >= 0, "Expected session to contain an event with hint 'page.login.username'");

            // Diagnostic: ensure TryGetTestIdFromSession extracts data-testid for this event
            var ui = BuildUiMap(new[] { ("login", "username", "page.login.username") });
            var resolver = new SemanticResolver(ui, session, 5, "draft.feature", "ui-map.yaml");
            var mi = typeof(SemanticResolver).GetMethod("TryGetTestIdFromSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var extracted = (string?)mi.Invoke(resolver, new object[] { idx });
            Assert.Equal("page.login.username", extracted);

            var draft = "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu preencho \"[data-testid='page.login.username']\" com \"x\"\n";
            var metadata = BuildDraftMetadata((idx, 7));

            var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draft, metadata);

            var s = resolvedMeta.Steps[0];
            Assert.Equal("resolved", s.Status);
            Assert.NotNull(s.Chosen);
            Assert.Equal("page.login.username", s.Chosen!.TestId);
            Assert.Empty(report.Findings.Where(f => f.DraftLine == 7));
        }
    }
}
