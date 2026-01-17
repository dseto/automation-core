using Automation.Core.Resolution;
using FluentAssertions;
using Xunit;

namespace Automation.Core.Tests;

public class LocatorFactoryTests
{
    [Fact]
    public void CreatesCssByTestId()
    {
        LocatorFactory.CssByTestId("x").Should().Be("[data-testid='x']");
    }
}
