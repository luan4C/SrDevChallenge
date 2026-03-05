using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;

namespace SIEG.SrDevChallenge.ArchitectureTests;

[TestFixture]
public class NamingConventionTests : ArchitectureTestBase
{
    [Test]
    public void Interfaces_Should_StartWithI()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All interfaces should start with 'I'");
    }


    [Test]
    public void Controllers_Should_HaveControllerSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All controllers should have 'Controller' suffix");
    }

    [Test]
    public void Services_Should_HaveServiceSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Services")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Service")
            .Or()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Service classes should have 'Service' or 'Validator' suffix");
    }

    [Test]
    public void Endpoints_Should_HaveEndpointsSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{ApiNamespace}.Endpoints")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Endpoints")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Endpoint classes should have 'Endpoints' suffix");
    }

    [Test]
    public void Constants_Should_BeStatic()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .HaveNameEndingWith("Constants")
            .Should()
            .BeStatic()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Constant classes should be static");
    }


    [Test]
    public void EventClasses_Should_BePastTense()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Criado")
            .Or()
            .HaveNameEndingWith("Atualizado")
            .Or()
            .HaveNameEndingWith("Removido")
            .Or()
            .HaveNameEndingWith("Event")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Event classes should be named in past tense (Portuguese) or end with 'Event'");
    }

    [Test]
    public void RepositoryInterfaces_Should_StartWithI()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Contracts")
            .And()
            .HaveNameEndingWith("Repository")
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "Repository interfaces should start with 'I'");
    }
}