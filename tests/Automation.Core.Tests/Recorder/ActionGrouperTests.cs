using System.Collections.Generic;
using Automation.Core.Recorder;
using Automation.Core.Recorder.Draft;
using Xunit;

namespace Automation.Core.Tests.Recorder;

public class ActionGrouperTests
{
    [Fact]
    public void Group_ClickFollowedByFillOnSameHint_UsesFillAsPrimary()
    {
        var session = new RecorderSession
        {
            Events = new List<RecorderEvent>
            {
                new RecorderEvent { T = "00:01.000", Type = "click", Target = Target("[data-testid='page.login.username']") },
                new RecorderEvent { T = "00:01.500", Type = "fill", Target = Target("[data-testid='page.login.username']"), Value = new Dictionary<string, object?> { ["literal"] = "admin" } }
            }
        };

        var grouper = new ActionGrouper();
        var groups = grouper.Group(session);

        Assert.Single(groups);
        Assert.Equal("fill", groups[0].PrimaryEvent?.Type);
    }

    [Fact]
    public void Group_ClickSubmitFollowedBySubmit_PrefersClick()
    {
        var session = new RecorderSession
        {
            Events = new List<RecorderEvent>
            {
                new RecorderEvent { T = "00:02.000", Type = "click", Target = Target("[data-testid='page.login.submit']") },
                new RecorderEvent { T = "00:02.400", Type = "submit", Target = Target("form ('Login')") }
            }
        };

        var grouper = new ActionGrouper();
        var groups = grouper.Group(session);

        Assert.Single(groups);
        Assert.Equal("click", groups[0].PrimaryEvent?.Type);
    }

    [Fact]
    public void Group_GenericClickThenSpecificClick_DropsGenericAsPrimary()
    {
        var session = new RecorderSession
        {
            Events = new List<RecorderEvent>
            {
                new RecorderEvent { T = "00:03.000", Type = "click", Target = Target("div") },
                new RecorderEvent { T = "00:03.500", Type = "click", Target = Target("[data-testid='page.login.username']") }
            }
        };

        var grouper = new ActionGrouper();
        var groups = grouper.Group(session);

        Assert.Single(groups);
        Assert.Equal("[data-testid='page.login.username']", Hint(groups[0].PrimaryEvent));
    }

    private static Dictionary<string, object?> Target(string hint)
    {
        return new Dictionary<string, object?> { ["hint"] = hint };
    }

    private static string? Hint(RecorderEvent? ev)
    {
        if (ev?.Target is Dictionary<string, object?> dict && dict.TryGetValue("hint", out var hint))
            return hint?.ToString();

        return null;
    }
}
