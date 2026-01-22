using System.IO;
using Automation.Validator.Validators;
using Xunit;

namespace Automation.Core.Tests
{
    public class ValidatorIntegrationTests
    {
        [Fact]
        public void CanonicalExamples_Pass_Validators()
        {
            var baseDir = Path.Combine("specs", "api", "examples");
            var resolved = Path.Combine(baseDir, "resolved.metadata.example.json");
            var uiGaps = Path.Combine(baseDir, "ui-gaps.report.example.json");

            var resolvedValidator = new ResolvedMetadataValidator();
            var r = resolvedValidator.Validate(resolved);
            Assert.True(r.IsValid, string.Join(";", r.Errors));

            var uiValidator = new UiGapsReportValidator();
            var u = uiValidator.Validate(uiGaps);
            Assert.True(u.IsValid, string.Join(";", u.Errors));
        }
    }
}
