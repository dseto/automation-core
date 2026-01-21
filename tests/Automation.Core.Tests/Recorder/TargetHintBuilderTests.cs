using System.Collections.Generic;
using Automation.Core.Recorder;
using Xunit;

namespace Automation.Core.Tests.Recorder;

public class TargetHintBuilderTests
{
    [Fact]
    public void BuildHint_UsesDataTestIdWhenPresent()
    {
        var target = new Dictionary<string, object>
        {
            ["tag"] = "input",
            ["attributes"] = new Dictionary<string, object>
            {
                ["data-testid"] = "page.login.username",
                ["id"] = "mat-input-0",
                ["formcontrolname"] = "username"
            }
        };

        var hint = TargetHintBuilder.BuildHint(target);

        Assert.Equal("[data-testid='page.login.username']", hint);
    }

    [Fact]
    public void BuildHint_FallsBackToStableAttributeWhenNoDataTestId()
    {
        var target = new Dictionary<string, object>
        {
            ["tag"] = "input",
            ["attributes"] = new Dictionary<string, object>
            {
                ["id"] = "mat-input-0",
                ["formcontrolname"] = "username"
            }
        };

        var hint = TargetHintBuilder.BuildHint(target);

        Assert.Equal("[formcontrolname='username']", hint);
    }
}
