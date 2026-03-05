using System.Reflection;
using NetArchTest.Rules;
using SIEG.SrDevChallenge.Api;
using SIEG.SrDevChallenge.Application;
using SIEG.SrDevChallenge.Domain;
using SIEG.SrDevChallenge.Infrastructure;

namespace SIEG.SrDevChallenge.ArchitectureTests;

public abstract class ArchitectureTestBase
{
    protected static readonly Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(InfrastructureAssemblyMarker).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    protected static readonly string DomainNamespace = "SIEG.SrDevChallenge.Domain";
    protected static readonly string ApplicationNamespace = "SIEG.SrDevChallenge.Application";
    protected static readonly string InfrastructureNamespace = "SIEG.SrDevChallenge.Infrastructure";
    protected static readonly string ApiNamespace = "SIEG.SrDevChallenge.Api";
    protected static readonly string CrossCuttingNamespace = "SIEG.SrDevChallenge.CrossCutting";

    protected static Types GetTypesFromAssembly(Assembly assembly) =>
        Types.InAssembly(assembly);

    protected static PredicateList GetTypesFromNamespace(string namespaceName) =>
        Types.InCurrentDomain()
              .That()
              .ResideInNamespace(namespaceName);
}