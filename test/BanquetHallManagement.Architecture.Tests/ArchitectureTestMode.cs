namespace BanquetHallManagement.Architecture.Tests;

/// <summary>
/// Controls whether architecture rule violations fail the test run.
/// Keep <see cref="EnforceStrictRules"/> false until the DDD migration baseline is clean.
/// </summary>
public static class ArchitectureTestMode
{
    public const bool EnforceStrictRules = false;
}
