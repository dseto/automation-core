using System.IO;
using Automation.Validator.Validators;
using Xunit;

namespace Automation.Core.Tests
{
    public class ResolvedFeatureValidatorTests
    {
        [Fact]
        public void Valid_resolved_feature_passes()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            var draft = Path.Combine(dir, "draft.feature");
            File.WriteAllText(draft, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"Login\"\n  Quando eu preencho \"login.username\" com \"demo\"\n  Quando eu clico em \"login.submit\"\n");

            var resolved = Path.Combine(dir, "resolved.feature");
            File.WriteAllText(resolved, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"Login\"\n  # UIGAP: UIGAP-0001 UI_MAP_KEY_NOT_FOUND — Element key não encontrada\n  Quando eu preencho \"login.username\" com \"demo\"\n  # UIGAP: UIGAP-0002 AMBIGUOUS_MATCH — Ambiguidade\n  Quando eu clico em \"login.submit\"\n");

            var meta = Path.Combine(dir, "resolved.metadata.json");
            File.WriteAllText(meta, "{\n  \"version\": \"1.0\",\n  \"generatedAt\": \"now\",\n  \"source\": { \"draftFeaturePath\": \"draft.feature\", \"uiMapPath\": \"uimap.yaml\", \"sessionPath\": null },\n  \"steps\": [\n    { \"draftLine\": 7, \"status\": \"resolved\", \"stepText\": \"  Dado que estou na página \\\"Login\\\"\", \"chosen\": { \"pageKey\": \"login\", \"elementKey\": \"page\", \"testId\": null }, \"findings\": [] },\n    { \"draftLine\": 8, \"status\": \"unresolved\", \"stepText\": \"  Quando eu preencho \\\"login.username\\\" com \\\"demo\\\"\", \"findings\": [ { \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"Element key não encontrada no UIMap\" } ] },\n    { \"draftLine\": 9, \"status\": \"partial\", \"stepText\": \"  Quando eu clico em \\\"login.submit\\\"\", \"findings\": [ { \"severity\": \"warn\", \"code\": \"AMBIGUOUS_MATCH\", \"message\": \"Ambiguidade\" } ], \"candidates\": [ { \"pageKey\": \"login\", \"elementKey\": \"submitPrimary\", \"testId\": \"btn-login\" } ] }\n  ]\n}");

            var validator = new ResolvedFeatureValidator();
            var res = validator.Validate(resolved, meta);
            Assert.True(res.IsValid, string.Join(";", res.Errors));

            Directory.Delete(dir, true);
        }

        [Fact]
        public void Missing_comment_fails()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            var draft = Path.Combine(dir, "draft.feature");
            File.WriteAllText(draft, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"Login\"\n  Quando eu preencho \"login.username\" com \"demo\"\n");

            var resolved = Path.Combine(dir, "resolved.feature");
            File.WriteAllText(resolved, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Dado que estou na página \"Login\"\n  Quando eu preencho \"login.username\" com \"demo\"\n");

            var meta = Path.Combine(dir, "resolved.metadata.json");
            File.WriteAllText(meta, "{\n  \"version\": \"1.0\",\n  \"generatedAt\": \"now\",\n  \"source\": { \"draftFeaturePath\": \"draft.feature\", \"uiMapPath\": \"uimap.yaml\", \"sessionPath\": null },\n  \"steps\": [\n    { \"draftLine\": 8, \"status\": \"unresolved\", \"stepText\": \"  Quando eu preencho \\\"login.username\\\" com \\\"demo\\\"\", \"findings\": [ { \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing\" } ] }\n  ]\n}");

            // write a ui-gaps report without comment in resolved feature
            var report = Path.Combine(dir, "ui-gaps.report.json");
            File.WriteAllText(report, "{ \"findings\": [ { \"id\": \"UIGAP-0001\", \"draftLine\": 8, \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing\" } ], \"stats\": { \"errors\":1, \"warnings\":0, \"infos\":0, \"total\":1 } }");

            var validator = new ResolvedFeatureValidator();
            var res = validator.Validate(resolved, meta);
            Assert.False(res.IsValid, $"Expected validation to fail. Errors: {string.Join(";", res.Errors.Select(e => e.Code + ":" + e.Message))} Warnings: {string.Join(";", res.Warnings.Select(w => w.Code + ":" + w.Message))}");
            Assert.Contains(res.Errors, e => e.Code == "UIGAPS_COMMENT_MISSING" || e.Code == "UIGAPS_COMMENT_INVALID");

            Directory.Delete(dir, true);
        }

        [Fact]
        public void DuplicatedSteps_CommentMatching_UsesDraftLineMapping()
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            // Draft with two identical steps
            var draft = Path.Combine(dir, "draft.feature");
            File.WriteAllText(draft, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  Quando eu clico em \"foo\"\n  Quando eu clico em \"foo\"\n");

            // Resolved feature with comments above each occurrence, using two different UIGAP ids
            var resolved = Path.Combine(dir, "resolved.feature");
            File.WriteAllText(resolved, "#language: pt\n\nFuncionalidade: X\n\nCenário: Y\n\n  # UIGAP: UIGAP-0001 UI_MAP_KEY_NOT_FOUND — missing A\n  Quando eu clico em \"foo\"\n  # UIGAP: UIGAP-0002 UI_MAP_KEY_NOT_FOUND — missing B\n  Quando eu clico em \"foo\"\n");

            // Metadata with steps referencing the correct draft lines
            var meta = Path.Combine(dir, "resolved.metadata.json");
            File.WriteAllText(meta, "{\n  \"version\": \"1.0\",\n  \"generatedAt\": \"now\",\n  \"source\": { \"draftFeaturePath\": \"draft.feature\", \"uiMapPath\": \"uimap.yaml\", \"sessionPath\": null },\n  \"steps\": [\n    { \"draftLine\": 7, \"status\": \"unresolved\", \"stepText\": \"  Quando eu clico em \\\"foo\\\"\", \"findings\": [ { \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing A\" } ] },\n    { \"draftLine\": 8, \"status\": \"unresolved\", \"stepText\": \"  Quando eu clico em \\\"foo\\\"\", \"findings\": [ { \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing B\" } ] }\n  ]\n}");

            // ui-gaps report with matching ids and draftLines
            var report = Path.Combine(dir, "ui-gaps.report.json");
            File.WriteAllText(report, "{ \"findings\": [ { \"id\": \"UIGAP-0001\", \"draftLine\": 7, \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing A\" }, { \"id\": \"UIGAP-0002\", \"draftLine\": 8, \"severity\": \"error\", \"code\": \"UI_MAP_KEY_NOT_FOUND\", \"message\": \"missing B\" } ], \"stats\": { \"errors\":2, \"warnings\":0, \"infos\":0, \"total\":2 } }");

            var validator = new ResolvedFeatureValidator();
            var res = validator.Validate(resolved, meta);
            Assert.True(res.IsValid, string.Join(";", res.Errors.Select(e => e.Code + ":" + e.Message)));

            Directory.Delete(dir, true);
        }
    }
}
