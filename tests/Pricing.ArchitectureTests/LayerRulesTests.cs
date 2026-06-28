using System.Reflection;
using NetArchTest.Rules;

namespace Pricing.ArchitectureTests;

public class LayerRulesTests
{
    private static readonly IReadOnlyList<Assembly> Assemblies = AssemblyRegistry.Pricing;

    [Fact]
    public void DomainTypes_ShouldNot_DependOn_ApplicationInfrastructureOrApi()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Domain")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Application", "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Api",
                "Pricing.Import.Application", "Pricing.Import.Infrastructure", "Pricing.Import.Api",
                "Pricing.Rating.Application", "Pricing.Rating.Infrastructure", "Pricing.Rating.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ApplicationTypes_ShouldNot_DependOn_InfrastructureOrApi()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Application")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Api",
                "Pricing.Import.Infrastructure", "Pricing.Import.Api",
                "Pricing.Rating.Infrastructure", "Pricing.Rating.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ContractsTypes_ShouldNot_DependOn_DomainApplicationInfrastructureOrFacade()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Contracts")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Domain", "Pricing.Inventory.Application", "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Facade",
                "Pricing.Import.Domain", "Pricing.Import.Application", "Pricing.Import.Infrastructure", "Pricing.Import.Facade",
                "Pricing.Rating.Domain", "Pricing.Rating.Application", "Pricing.Rating.Infrastructure", "Pricing.Rating.Facade"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void FacadeTypes_ShouldNot_DependOn_ModuleDomainApplicationInfrastructureOrApi()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespaceContaining(".Facade")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Domain", "Pricing.Inventory.Application", "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Api",
                "Pricing.Import.Domain", "Pricing.Import.Application", "Pricing.Import.Infrastructure", "Pricing.Import.Api",
                "Pricing.Rating.Domain", "Pricing.Rating.Application", "Pricing.Rating.Infrastructure", "Pricing.Rating.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
