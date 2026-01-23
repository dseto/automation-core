using System.Collections.Generic;
using Xunit;
using Automation.Core.UiMap;

namespace Automation.Core.Tests
{
    public class UiMapValidatorInvalidElementKeysTests
    {
        [Fact]
        public void Validate_Fails_When_ElementKeyContainsDot()
        {
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/",
                    ["anchor"] = "page.home"
                },
                ["login.submit"] = "login.submit"
            };
            ui.Pages["home"] = pageDict;

            var ex = Assert.Throws<UiMapValidationException>(() => UiMapValidator.Validate(ui, "test-uimap"));
            Assert.Contains("contains invalid character '.' in element key", ex.Message);
        }
    }
}
