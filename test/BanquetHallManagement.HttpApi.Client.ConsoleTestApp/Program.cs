using System;
using System.Threading.Tasks;
using BanquetHallManagement.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;

namespace BanquetHallManagement.HttpApi.Client.ConsoleTestApp;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = BuildConfiguration();
        BanquetHallManagementSecretsValidator.ValidateForConsoleTestClient(configuration);

        using (var application = await AbpApplicationFactory.CreateAsync<BanquetHallManagementConsoleApiClientModule>(options =>
        {
           options.Services.ReplaceConfiguration(configuration);
           options.UseAutofac();
        }))
        {
            await application.InitializeAsync();

            var demo = application.ServiceProvider.GetRequiredService<ClientDemoService>();
            await demo.RunAsync();

            Console.WriteLine("Press ENTER to stop application...");
            Console.ReadLine();

            await application.ShutdownAsync();
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.secrets.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
