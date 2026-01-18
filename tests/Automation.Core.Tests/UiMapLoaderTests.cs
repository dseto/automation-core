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
  meta:
    route: /login
    anchor: login-title
  elements:
    Usuario: login-user
    Senha: login-pass
";
        var temp = Path.Combine(Path.GetTempPath(), $"ui-map-{Guid.NewGuid():N}.yaml");
        UiMapModel m;
        try
        {
            File.WriteAllText(temp, yaml);
            m = UiMapLoader.LoadFromFile(temp);
            m.Pages.Should().ContainKey("LoginPage");
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
        m.GetPageOrThrow("LoginPage").Elements.Should().ContainKey("Usuario");
        m.GetPageOrThrow("LoginPage").Meta!.Route.Should().Be("/login");
    }

        [Fact]
        public void LoadsInlineElementsWithMetaAndTestIdMappings()
        {
                var yaml = @"
pages:
    login:
        __meta:
            route:
                testId: /login
            anchor:
                testId: page.login.anchor
        username:
            testId: page.login.username
        password:
            testId: page.login.password
";
                var temp = Path.Combine(Path.GetTempPath(), $"ui-map-{Guid.NewGuid():N}.yaml");
                UiMapModel m;
                try
                {
                        File.WriteAllText(temp, yaml);
                        m = UiMapLoader.LoadFromFile(temp);
                }
                finally
                {
                        if (File.Exists(temp)) File.Delete(temp);
                }

                m.Pages.Should().ContainKey("login");
                m.GetPageOrThrow("login").Elements.Should().ContainKey("username");
                m.GetPageOrThrow("login").Elements["password"].Should().Be("page.login.password");
                m.GetPageOrThrow("login").Meta!.Route.Should().Be("/login");
                m.GetPageOrThrow("login").Meta!.Anchor.Should().Be("page.login.anchor");
        }

        [Fact]
        public void FailsWhenPageHasNoElements()
        {
                var yaml = @"
LoginPage:
    __meta:
        route: /login
";
                var temp = Path.Combine(Path.GetTempPath(), $"ui-map-{Guid.NewGuid():N}.yaml");
                try
                {
                        File.WriteAllText(temp, yaml);
                        Action act = () => UiMapLoader.LoadFromFile(temp);
                        act.Should().Throw<UiMapValidationException>()
                                .WithMessage("*has 0 elements mapped*");
                }
                finally
                {
                        if (File.Exists(temp)) File.Delete(temp);
                }
        }
}
