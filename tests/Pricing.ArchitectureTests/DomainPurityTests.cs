using System.Reflection;
using NetArchTest.Rules;

namespace Pricing.ArchitectureTests;

public class DomainPurityTests
{
    private static readonly IReadOnlyList<Assembly> Assemblies = AssemblyRegistry.Pricing;

    [Fact]
    public void DomainTypes_ShouldNot_DependOn_EntityFrameworkCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void DomainTypes_ShouldNot_DependOn_AspNetCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ApplicationTypes_ShouldNot_DependOn_EntityFrameworkCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Application")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ApplicationTypes_ShouldNot_DependOn_AspNetCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Application")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
