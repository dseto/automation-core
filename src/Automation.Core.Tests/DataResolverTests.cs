using System.Collections;
using System.Collections.Generic;
using Automation.Core.Configuration;
using Automation.Core.DataMap;
using Xunit;

namespace Automation.Core.Tests;

public class DataResolverTests
{
    [Fact]
    public void Resolve_ObjectReference_ReturnsObject_WhenFound()
    {
        var model = new DataMapModel();
        var defaultContext = new Hashtable();
        var userAdmin = new Hashtable();
        userAdmin["username"] = "admin";
        userAdmin["password"] = "ChangeMe123!";
        defaultContext["user_admin"] = userAdmin;
        model.Contexts["default"] = defaultContext;

        var settings = RunSettings.FromEnvironment();
        var resolver = new DataResolver(model, settings);

        var result = resolver.Resolve("@user_admin");

        Assert.IsType<Hashtable>(result);
        var h = Assert.IsType<Hashtable>(result);
        Assert.Equal("admin", h["username"]);
    }

    [Fact]
    public void Resolve_LiteralStartingWithAt_IsTreatedAsLiteral_WhenNotFound()
    {
        var model = new DataMapModel();
        var settings = RunSettings.FromEnvironment();
        var resolver = new DataResolver(model, settings);

        var input = "@ChangeMe123!";
        var result = resolver.Resolve(input);

        Assert.IsType<string>(result);
        Assert.Equal(input, result);
    }
}
