using System.Collections.Generic;
using Xunit;
using Automation.Core.UiMap;

namespace Automation.Core.Tests
{
    public class UiMapValidatorTests
    {
        [Fact]
        public void Validate_Passes_When_RootRoutePresent()
        {
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/",
                    ["anchor"] = "page.home"
                },
                ["page"] = "page.home.page"
            };
            ui.Pages["home"] = pageDict;

            // Should not throw
            UiMapValidator.Validate(ui, "test-uimap");
        }

        [Fact]
        public void Validate_Fails_When_RootRouteMissing()
        {
            var ui = new UiMapModel();
            var pageDict = new Dictionary<string, object>
            {
                ["__meta"] = new Dictionary<string, object>
                {
                    ["route"] = "/login",
                    ["anchor"] = "page.login"
                },
                ["username"] = "page.login.username"
            };
            ui.Pages["login"] = pageDict;

            Assert.Throws<UiMapValidationException>(() => UiMapValidator.Validate(ui, "test-uimap"));
        }
    }
}
