using System;
using Microsoft.Extensions.Configuration;

namespace BanquetHallManagement.Configuration;

/// <summary>
/// Validates that required secrets are supplied via user secrets, environment variables,
/// or appsettings.secrets.json — never from committed appsettings.json.
/// </summary>
public static class BanquetHallManagementSecretsValidator
{
    public static void ValidateForDbMigrator(IConfiguration configuration)
    {
        Require(configuration, BanquetHallManagementConfigurationKeys.ConnectionStrings.Default);
        Require(configuration, BanquetHallManagementConfigurationKeys.Seed.AbpAdminPassword);
    }

    public static void ValidateForHttpApiHost(IConfiguration configuration, bool isDevelopment)
    {
        Require(configuration, BanquetHallManagementConfigurationKeys.ConnectionStrings.Default);
        Require(configuration, BanquetHallManagementConfigurationKeys.StringEncryption.DefaultPassPhrase);

        if (!isDevelopment)
        {
            Require(configuration, BanquetHallManagementConfigurationKeys.AuthServer.CertificatePassPhrase);
        }
    }

    public static void ValidateForConsoleTestClient(IConfiguration configuration)
    {
        Require(configuration, "IdentityClients:Default:UserPassword");
    }

    private static void Require(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var environmentKey = key.Replace(':', '_');
        throw new InvalidOperationException(
            $"Required configuration '{key}' is missing. " +
            $"Set it in appsettings.secrets.json (gitignored), .NET user secrets, " +
            $"or an environment variable such as '{environmentKey}'. " +
            "See appsettings.secrets.json.example and README.md.");
    }
}
