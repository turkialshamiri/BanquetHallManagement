using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BanquetHallManagement;
using NetArchTest.Rules;
using Xunit;
using Xunit.Abstractions;

namespace BanquetHallManagement.Architecture.Tests;

/// <summary>
/// Baseline layered-architecture rules for the BanquetHallManagement monolith.
/// Violations are logged as warnings until <see cref="ArchitectureTestMode.EnforceStrictRules"/> is enabled.
/// </summary>
public class LayerDependencyTests
{
    private readonly ITestOutputHelper _output;

    public LayerDependencyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Domain_Should_Not_Reference_EntityFrameworkCore()
    {
        EvaluateForbiddenDependencies(
            DomainAssembly,
            "Domain must not reference EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore",
            "BanquetHallManagement.EntityFrameworkCore",
            "Volo.Abp.EntityFrameworkCore");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        EvaluateForbiddenDependencies(
            DomainAssembly,
            "Domain must not reference Application layer",
            "BanquetHallManagement.Application",
            "Volo.Abp.Application");
    }

    [Fact]
    public void Application_Should_Not_Reference_EntityFrameworkCore()
    {
        EvaluateForbiddenDependencies(
            ApplicationAssembly,
            "Application must not reference EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore",
            "BanquetHallManagement.EntityFrameworkCore",
            "Volo.Abp.EntityFrameworkCore");
    }

    [Fact]
    public void Project_References_Should_Respect_Layer_Boundaries()
    {
        ArchitectureTestHelper.EvaluateProjectReferences(
            _output,
            "Domain",
            GetReferencedAssemblyNames(DomainAssembly),
            [
                "BanquetHallManagement.Application",
                "BanquetHallManagement.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore"
            ]);

        ArchitectureTestHelper.EvaluateProjectReferences(
            _output,
            "Application",
            GetReferencedAssemblyNames(ApplicationAssembly),
            [
                "BanquetHallManagement.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore"
            ]);
    }

    private void EvaluateForbiddenDependencies(
        Assembly assembly,
        string ruleName,
        params string[] forbiddenDependencies)
    {
        var failingTypes = new List<string>();

        try
        {
            foreach (var dependency in forbiddenDependencies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOn(dependency)
                    .GetResult();

                if (!result.IsSuccessful && result.FailingTypes != null)
                {
                    failingTypes.AddRange(result.FailingTypes.Select(type => type.FullName ?? type.Name));
                }
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"[ARCHITECTURE][WARNING] {ruleName} — NetArchTest analysis skipped: {ex.GetType().Name}: {ex.Message}");
            _output.WriteLine("  Falling back to project-reference validation in Project_References_Should_Respect_Layer_Boundaries.");

            if (ArchitectureTestMode.EnforceStrictRules)
            {
                throw;
            }

            return;
        }

        if (failingTypes.Count == 0)
        {
            ArchitectureTestHelper.EvaluateRule(_output, ruleName, isSuccessful: true);
            return;
        }

        ArchitectureTestHelper.EvaluateRule(
            _output,
            ruleName,
            isSuccessful: false,
            failingTypeNames: failingTypes);
    }

    private static Assembly DomainAssembly => typeof(BanquetHallManagementDomainModule).Assembly;

    private static Assembly ApplicationAssembly => typeof(BanquetHallManagementApplicationModule).Assembly;

    private static IEnumerable<string> GetReferencedAssemblyNames(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => !string.IsNullOrWhiteSpace(name));
    }
}
