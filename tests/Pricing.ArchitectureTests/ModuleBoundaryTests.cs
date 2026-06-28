using System.Reflection;
using NetArchTest.Rules;

namespace Pricing.ArchitectureTests;

public class ModuleBoundaryTests
{
    private static readonly IReadOnlyList<Assembly> Assemblies = AssemblyRegistry.Pricing;

    [Fact]
    public void InventoryDomain_ShouldNot_DependOn_ImportOrRating()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Inventory.Domain")
            .ShouldNot().HaveDependencyOnAny("Pricing.Import", "Pricing.Rating")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ImportDomain_ShouldNot_DependOn_InventoryOrRating()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Import.Domain")
            .ShouldNot().HaveDependencyOnAny("Pricing.Inventory", "Pricing.Rating")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void RatingDomain_ShouldNot_DependOn_InventoryOrImport()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Rating.Domain")
            .ShouldNot().HaveDependencyOnAny("Pricing.Inventory", "Pricing.Import")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void InventoryApplication_ShouldNot_DependOn_OtherModules_ExceptFacade()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Inventory.Application")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Import.Domain", "Pricing.Import.Application", "Pricing.Import.Infrastructure", "Pricing.Import.Api",
                "Pricing.Rating.Domain", "Pricing.Rating.Application", "Pricing.Rating.Infrastructure", "Pricing.Rating.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void ImportApplication_ShouldNot_DependOn_OtherModules_ExceptFacade()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Import.Application")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Domain", "Pricing.Inventory.Application", "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Api",
                "Pricing.Rating.Domain", "Pricing.Rating.Application", "Pricing.Rating.Infrastructure", "Pricing.Rating.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void RatingApplication_ShouldNot_DependOn_OtherModules_ExceptFacade()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().ResideInNamespace("Pricing.Rating.Application")
            .ShouldNot().HaveDependencyOnAny(
                "Pricing.Inventory.Domain", "Pricing.Inventory.Application", "Pricing.Inventory.Infrastructure", "Pricing.Inventory.Api",
                "Pricing.Import.Domain", "Pricing.Import.Application", "Pricing.Import.Infrastructure", "Pricing.Import.Api"
            )
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []);
}
