using System.Reflection;
using Mono.Cecil;
using NetArchTest.Rules;

namespace Pricing.ArchitectureTests;

public class NamingConventionTests
{
    private static readonly IReadOnlyList<Assembly> Assemblies = AssemblyRegistry.Pricing;

    [Fact]
    public void UseCaseClasses_ShouldBe_Sealed()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().HaveNameEndingWith("UseCase").And().AreClasses()
            .Should().BeSealed()
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void RepositoryInterfaces_ShouldResideIn_DomainNamespace()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().AreInterfaces().And().HaveNameEndingWith("Repository")
            .Should().ResideInNamespaceContaining(".Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void EndpointClasses_ShouldInheritFrom_FastEndpointsEndpoint()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().HaveNameEndingWith("Endpoint").And().AreClasses()
            .Should().MeetCustomRule(new InheritsFromFastEndpointRule())
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    [Fact]
    public void InfrastructureConfigurationClasses_ShouldImplement_IEntityTypeConfiguration()
    {
        var result = Types.InAssemblies(Assemblies)
            .That().HaveNameEndingWith("Configuration").And().AreClasses()
            .And().ResideInNamespaceContaining(".Infrastructure")
            .Should().MeetCustomRule(new ImplementsEntityTypeConfigurationRule())
            .GetResult();

        Assert.True(result.IsSuccessful, FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []);

    private sealed class InheritsFromFastEndpointRule : ICustomRule
    {
        public bool MeetsRule(TypeDefinition type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.Namespace == "FastEndpoints")
                    return true;
                try { current = current.Resolve()?.BaseType; }
                catch { return false; }
            }
            return false;
        }
    }

    private sealed class ImplementsEntityTypeConfigurationRule : ICustomRule
    {
        public bool MeetsRule(TypeDefinition type) =>
            type.Interfaces.Any(i =>
                i.InterfaceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore.IEntityTypeConfiguration`") == true);
    }
}
