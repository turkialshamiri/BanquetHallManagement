# BanquetHallManagement

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

### Pre-requirements

* [.NET10.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)

### Configurations

Secrets are **not** stored in tracked `appsettings.json` files. Before running locally:

1. Run `.\etc\scripts\init-local-secrets.ps1` (or manually copy each `appsettings.secrets.json.example` to `appsettings.secrets.json`).
2. Fill in connection strings, certificate passphrase, encryption passphrase, and seed passwords in the gitignored `appsettings.secrets.json` files under:
   - `src/BanquetHallManagement.HttpApi.Host/`
   - `src/BanquetHallManagement.DbMigrator/`
   - `test/BanquetHallManagement.HttpApi.Client.ConsoleTestApp/` (console test client only)
3. Alternatively, use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) (`UserSecretsId` is configured on Host, DbMigrator, and ConsoleTestApp) or environment variables.

`appsettings.secrets.json` is gitignored and is never published to production. Host and DbMigrator fail fast at startup if required secrets are missing.

| Configuration key | Environment variable | Required by |
| --- | --- | --- |
| `ConnectionStrings:Default` | `ConnectionStrings__Default` | Host, DbMigrator |
| `StringEncryption:DefaultPassPhrase` | `StringEncryption__DefaultPassPhrase` | Host |
| `AuthServer:CertificatePassPhrase` | `AuthServer__CertificatePassPhrase` | Host (production) |
| `Seed:AbpAdmin:Password` | `Seed__AbpAdmin__Password` | DbMigrator |
| `Seed:ApplicationAdmin:*` | `Seed__ApplicationAdmin__*` | DbMigrator (optional seed) |
| `IdentityClients:Default:UserPassword` | `IdentityClients__Default__UserPassword` | ConsoleTestApp |

User-secrets template for the API host: `etc/secrets/user-secrets.host.template.json`.

### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `BanquetHallManagement.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

#### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate. ABP Framework sets up signing and encryption certificates in your application and expects an `openiddict.pfx` file in your application.

To generate a signing certificate, you can use the following command:

```bash
# Use the same passphrase as AuthServer:CertificatePassPhrase in appsettings.secrets.json
dotnet dev-certs https -v -ep openiddict.pfx -p "$AUTH_SERVER_CERTIFICATE_PASSPHRASE"
```

> Set `AUTH_SERVER_CERTIFICATE_PASSPHRASE` (or `AuthServer__CertificatePassPhrase`) to a strong value and keep it in user secrets / your deployment secret store — never commit it.

It is recommended to use **two** RSA certificates, distinct from the certificate(s) used for HTTPS: one for encryption, one for signing.

For more information, please refer to: [OpenIddict Certificate Configuration](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html#registering-a-certificate-recommended-for-production-ready-scenarios)

> Also, see the [Configuring OpenIddict](https://abp.io/docs/latest/Deployment/Configuring-OpenIddict#production-environment) documentation for more information.

### Solution structure

This is a layered monolith application that consists of the following applications:

* `angular`: Angular application.
* `BanquetHallManagement.DbMigrator`: A console application which applies the migrations and also seeds the initial data. It is useful on development as well as on production environment.
* `BanquetHallManagement.HttpApi.Host`: ASP.NET Core API application that is used to expose the APIs to the clients.

#### Test Projects

The `test` folder contains the following test projects:

* `BanquetHallManagement.Application.Tests`: Application layer tests.
* `BanquetHallManagement.Domain.Tests`: Domain layer tests.
* `BanquetHallManagement.EntityFrameworkCore.Tests`: Entity Framework Core integration tests.




## Deploying the application

Deploying an ABP application follows the same process as deploying any .NET or ASP.NET Core application. However, there are important considerations to keep in mind. For detailed guidance, refer to ABP's [deployment documentation](https://abp.io/docs/latest/Deployment/Index).

**Production secrets:** inject configuration via your platform secret store (Azure Key Vault, AWS Secrets Manager, Kubernetes secrets, etc.) as environment variables — never ship `appsettings.secrets.json`. Minimum required:

- `ConnectionStrings__Default` — SQL Server connection string
- `StringEncryption__DefaultPassPhrase` — stable passphrase (changing it invalidates encrypted data)
- `AuthServer__CertificatePassPhrase` — passphrase for `openiddict.pfx` placed beside the Host binary
- `Seed__AbpAdmin__Password` — only when running DbMigrator for initial seed

Set `ASPNETCORE_ENVIRONMENT=Production` so `appsettings.Production.json` disables identity PII logging.

### Additional resources


#### Internal Resources

You can find detailed setup and configuration guide(s) for your solution below:

* [Angular](./angular/README.md)

#### External Resources
You can see the following resources to learn more about your solution and the ABP Framework:

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
