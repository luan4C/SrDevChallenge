using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using NUnit.Framework;
using SIEG.SrDevChallenge.Application.Contracts;

namespace SIEG.SrDevChallenge.ArchitectureTests;

[TestFixture]
public class ArchitecturalPatternsTests : ArchitectureTestBase
{
    [Test]
    public void Handlers_Should_HaveHandlerSuffix()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<>))
            .Or()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All MediatR handlers should have 'Handler' suffix");
    }

    [Test]
    public void Commands_Should_HaveCommandSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.features.Commands")
            .And()
            .ImplementInterface(typeof(IRequest))
            .Or()
            .ImplementInterface(typeof(IRequest<>))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All commands should have 'Command' suffix");
    }

    [Test]
    public void Queries_Should_HaveQuerySuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.features.Queries")
            .And()
            .ImplementInterface(typeof(IRequest<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All queries should have 'Query' suffix");
    }

    [Test]
    public void Repositories_Should_HaveRepositorySuffix()
    {
        // Arrange & Act
        var result = GetTypesFromAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IRepository<>)) 
            .And().AreNotAbstract()                       
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All repository implementations should have 'Repository' suffix");
    }

    [Test]
    public void Entities_Should_BeInEntitiesNamespace()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .Should()
            .BeClasses()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All entities should be in the Entities namespace");
    }

    [Test]
    public void Exceptions_Should_InheritFromException()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Exceptions")
            .Should()
            .Inherit(typeof(Exception))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All custom exceptions should inherit from Exception");
    }

    [Test]
    public void Exceptions_Should_HaveExceptionSuffix()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Exceptions")
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All exception classes should have 'Exception' suffix");
    }


    [Test]
    public void Events_Should_BeInEventsNamespace()
    {
        // Arrange & Act
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .Should()
            .BeClasses()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: "All events should be in the Events namespace");
    }

    [Test]
    public void Enums_Should_BeInEnumsNamespace()
    {
        // Arrange & Act
        var typesInEnumsNamespace = Types.InCurrentDomain()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Enums")
            .GetTypes();
        
        // Assert
        typesInEnumsNamespace.Should().NotBeEmpty(
            because: "There should be enums in the Enums namespace");
        
        typesInEnumsNamespace.Should().OnlyContain(t => t.IsEnum,
            because: "All types in Enums namespace should be enums");
    }
}