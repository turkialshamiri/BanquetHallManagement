namespace BanquetHallManagement.Configuration;

/// <summary>
/// Configuration key names for secrets and environment-specific settings.
/// Store values in appsettings.secrets.json, user secrets, or environment variables.
/// </summary>
public static class BanquetHallManagementConfigurationKeys
{
    public static class ConnectionStrings
    {
        public const string Default = "ConnectionStrings:Default";
    }

    public static class AuthServer
    {
        public const string CertificatePassPhrase = "AuthServer:CertificatePassPhrase";
    }

    public static class StringEncryption
    {
        public const string DefaultPassPhrase = "StringEncryption:DefaultPassPhrase";
    }

    public static class Seed
    {
        public const string AbpAdminEmail = "Seed:AbpAdmin:Email";

        public const string AbpAdminPassword = "Seed:AbpAdmin:Password";

        public const string ApplicationAdminEnabled = "Seed:ApplicationAdmin:Enabled";

        public const string ApplicationAdminUserName = "Seed:ApplicationAdmin:UserName";

        public const string ApplicationAdminEmail = "Seed:ApplicationAdmin:Email";

        public const string ApplicationAdminPassword = "Seed:ApplicationAdmin:Password";

        public const string ApplicationAdminName = "Seed:ApplicationAdmin:Name";

        public const string ApplicationAdminSurname = "Seed:ApplicationAdmin:Surname";

        public const string ApplicationAdminPhoneNumber = "Seed:ApplicationAdmin:PhoneNumber";
    }
}
