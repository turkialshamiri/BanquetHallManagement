using System.Collections.Generic;
using System.Linq;
using NetArchTest.Rules;
using Xunit;
using Xunit.Abstractions;

namespace BanquetHallManagement.Architecture.Tests;

internal static class ArchitectureTestHelper
{
    public static void EvaluateRule(
        ITestOutputHelper output,
        string ruleName,
        bool isSuccessful,
        IEnumerable<string>? failingTypeNames = null)
    {
        if (isSuccessful)
        {
            output.WriteLine($"[ARCHITECTURE][PASS] {ruleName}");
            return;
        }

        var violations = failingTypeNames?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList() ?? [];

        var violationDetails = violations.Count > 0
            ? string.Join(Environment.NewLine, violations.Select(name => $"  - {name}"))
            : "  (no failing types reported)";

        output.WriteLine($"[ARCHITECTURE][WARNING] {ruleName}");
        output.WriteLine(violationDetails);

        if (ArchitectureTestMode.EnforceStrictRules)
        {
            Assert.True(isSuccessful, $"{ruleName} failed. See test output for violating types.");
        }
    }

    public static void EvaluateRule(
        ITestOutputHelper output,
        string ruleName,
        TestResult result)
    {
        EvaluateRule(
            output,
            ruleName,
            result.IsSuccessful,
            result.FailingTypes?.Select(type => type.FullName ?? type.Name));
    }

    public static void EvaluateProjectReferences(
        ITestOutputHelper output,
        string sourceLayer,
        IEnumerable<string> referencedAssemblies,
        IEnumerable<string> forbiddenAssemblyNameFragments)
    {
        var violations = referencedAssemblies
            .Where(reference => forbiddenAssemblyNameFragments.Any(
                fragment => reference.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(reference => reference, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (violations.Count == 0)
        {
            output.WriteLine($"[ARCHITECTURE][PASS] {sourceLayer} project references respect layer boundaries.");
            return;
        }

        output.WriteLine($"[ARCHITECTURE][WARNING] {sourceLayer} project references contain forbidden dependencies:");
        foreach (var violation in violations)
        {
            output.WriteLine($"  - {violation}");
        }

        if (ArchitectureTestMode.EnforceStrictRules)
        {
            Assert.True(violations.Count == 0, $"{sourceLayer} has forbidden project references.");
        }
    }
}
