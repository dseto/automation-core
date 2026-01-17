using Automation.Core.UiMap;
using FluentAssertions;
using Xunit;

namespace Automation.Core.Tests;

public class UiMapLoaderTests
{
    [Fact]
    public void LoadsPagesAndElements()
    {
        var yaml = @"
LoginPage:
  __meta:
    route: /login
    anchor: login-title
  Usuario: login-user
  Senha: login-pass
";
        var m = new UiMapLoader().LoadFromString(yaml);
        m.Pages.Should().ContainKey("LoginPage");
        m.GetPageOrThrow("LoginPage").Elements.Should().ContainKey("Usuario");
        m.GetPageOrThrow("LoginPage").Meta!.Route.Should().Be("/login");
    }
}
