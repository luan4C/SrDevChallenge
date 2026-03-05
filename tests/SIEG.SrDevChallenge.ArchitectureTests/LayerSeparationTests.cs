using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;

namespace SIEG.SrDevChallenge.ArchitectureTests;

[TestFixture]
public class LayerSeparationTests : ArchitectureTestBase
{
    [Test]
    public void Domain_Should_Not_HaveDependencyOn_Application()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on Application layer");
    }

    [Test]
    public void Domain_Should_Not_HaveDependencyOn_Infrastructure()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on Infrastructure layer");
    }

    [Test]
    public void Domain_Should_Not_HaveDependencyOn_Api()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer should not depend on API layer");
    }

    [Test]
    public void Application_Should_Not_HaveDependencyOn_Infrastructure()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Application layer should not depend on Infrastructure layer - only on contracts/interfaces");
    }

    [Test]
    public void Application_Should_Not_HaveDependencyOn_Api()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Application layer should not depend on API layer");
    }

    [Test]
    public void Infrastructure_Should_Not_HaveDependencyOn_Api()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure layer should not depend on API layer");
    }

    [Test]
    public void Domain_Should_OnlyDependOn_SystemNamespaces()
    {
        // Arrange
        var allowedNamespaces = new[]
        {
            "System",
            DomainNamespace
        };

        // Act
        var result = GetTypesFromAssembly(DomainAssembly)
            .Should()
            .OnlyHaveDependenciesOn(allowedNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Domain should only depend on System namespaces and itself");
    }

   
}