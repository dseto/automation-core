using Xunit;

namespace Automation.Core.Tests
{
    public class RouteNormalizerTests
    {

        [Fact]
        public void Normalize_FilePathAndFragment_ReturnsTail()
        {
            var pathname = "/C:/Projetos/automation-core/ui-tests/pages/insurance-quote-spa-static/app.html";
            var fragment = "#/dashboard";
            var outp = Automation.Core.Recorder.RouteNormalizer.Normalize(null, pathname, fragment, null);
            Assert.Equal("/app.html#/dashboard", outp);
        }

        [Fact]
        public void Normalize_BaseUrlStripping_ReturnsTail()
        {
            var url = "http://localhost/project/app.html";
            var outp = Automation.Core.Recorder.RouteNormalizer.Normalize(url, null, null, "http://localhost/project");
            Assert.Equal("/app.html", outp);
        }

        [Fact]
        public void Normalize_HttpUrlWithFragment_ReturnsTail()
        {
            var url = "http://localhost/insurance-quote-spa-static/app.html#/dashboard";
            var outp = Automation.Core.Recorder.RouteNormalizer.Normalize(url, null, null, null);
            Assert.Equal("/app.html#/dashboard", outp);
        }

        [Fact]
        public void Normalize_PathnameSimple_ReturnsPath()
        {
            var pathname = "/login";
            var outp = Automation.Core.Recorder.RouteNormalizer.Normalize(null, pathname, null, null);
            Assert.Equal("/login", outp);
        }
    }
}
