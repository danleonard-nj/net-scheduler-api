namespace NetScheduler.Configuration.Extensions;

using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using MongoDB.Driver;
using NetScheduler.Clients;
using NetScheduler.Clients.Abstractions;
using NetScheduler.Configuration.Constants;
using NetScheduler.Configuration.Extensions;
using NetScheduler.Configuration.Settings;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Repositories;
using NetScheduler.Services.Cache;
using NetScheduler.Services.Cache.Abstractions;
using NetScheduler.Services.Events;
using NetScheduler.Services.Events.Abstractions;
using NetScheduler.Services.History;
using NetScheduler.Services.History.Abstractions;
using NetScheduler.Services.Identity;
using NetScheduler.Services.Identity.Abstractions;
using NetScheduler.Services.Schedules;
using NetScheduler.Services.Schedules.Abstractions;
using NetScheduler.Services.Tasks;
using NetScheduler.Services.Tasks.Abstractions;
using Serilog;

public static class ConfigurationExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder webApplicationBuilder)
    {
        var secrets = webApplicationBuilder.GetKeyVaultSecrets();

        webApplicationBuilder.RegisterMongo(secrets);
        webApplicationBuilder.RegisterSettings(secrets);
        webApplicationBuilder.ConfigureAuth();
        webApplicationBuilder.ConfigureAzureClients();

        webApplicationBuilder.Services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        webApplicationBuilder.Services.AddSingleton<IIdentityClientRepository, IdentityClientRepository>();
        webApplicationBuilder.Services.AddSingleton<IScheduleRepository, ScheduleRepository>();
        webApplicationBuilder.Services.AddSingleton<ITaskRepository, TaskRepository>();
        webApplicationBuilder.Services.AddSingleton<IScheduleHistoryRepository, ScheduleHistoryRepository>();
        webApplicationBuilder.Services.AddSingleton<IFeatureClient, FeatureClient>();
        webApplicationBuilder.Services.AddSingleton<ICacheService, CacheService>();

        webApplicationBuilder.Services.AddScoped<IIdentityService, IdentityService>();
        webApplicationBuilder.Services.AddScoped<IScheduleService, ScheduleService>();
        webApplicationBuilder.Services.AddScoped<ITaskService, TaskService>();
        webApplicationBuilder.Services.AddScoped<IEventService, EventService>();
        webApplicationBuilder.Services.AddScoped<IScheduleHistoryService, ScheduleHistoryService>();

        webApplicationBuilder.ConfigureCors();
        webApplicationBuilder.ConfigureLogging();
        webApplicationBuilder.ConfigureCache();
    }

    private static void RegisterMongo(this WebApplicationBuilder webApplicationBuilder, IDictionary<string, string> keyVaultSecrets)
    {
        var mongoConfiguration = webApplicationBuilder.Bind<MongoConfiguration>();
        mongoConfiguration.InjectSecrets(keyVaultSecrets);

        var mongoClient = new MongoClient(mongoConfiguration.ConnectionString);

        webApplicationBuilder.Services.AddSingleton<IMongoClient>(mongoClient);
    }

    private static void ConfigureCache(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(
            "Redis");

        builder.Services.AddStackExchangeRedisCache(config =>
        {
            config.Configuration = connectionString;
        });
    }

    private static void ConfigureCors(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddCors(config =>
        {
            config.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });
        });
    }

    private static void ConfigureLogging(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Host.UseSerilog((ctx, lc) =>
        {
            lc.WriteTo.Console();
            lc.ReadFrom.Configuration(ctx.Configuration);
        });

        IdentityModelEventSource.ShowPII = true;
        webApplicationBuilder.Services.AddLogging();
    }

    private static void ConfigureAzureClients(this WebApplicationBuilder webApplicationBuilder)
    {
        var serviceBusConnectionString = webApplicationBuilder
            .Configuration
            .GetConnectionString("ServiceBus");

        webApplicationBuilder.Services.AddAzureClients(clientBuilder =>
        {
            clientBuilder
                .AddServiceBusClient(serviceBusConnectionString)
                .WithName("ApiEvents");
        });
    }

    private static IDictionary<string, string> GetKeyVaultSecrets(this WebApplicationBuilder webApplicationBuilder)
    {
        var keyVaultSettings = webApplicationBuilder.Bind<KeyVaultSettings>();

        var client = new SecretClient(
            new Uri(keyVaultSettings.KeyVaultUrl),
            new DefaultAzureCredential());

        var secrets = client.GetPropertiesOfSecrets().ToList();
        var secretNames = secrets.Select(x => x.Name);

        var tasks = secretNames.Select(async secret => await client.GetSecretAsync(secret));
        var secretList = Task.WhenAll(tasks).Result;

        var dictionary = new Dictionary<string, string>();
        foreach (var secret in secretList)
        {
            dictionary.Add(secret.Value.Properties.Name, secret.Value.Value);
        }

        return dictionary;
    }

    private static void RegisterSettings(this WebApplicationBuilder webApplicationBuilder, IDictionary<string, string> secrets)
    {
        webApplicationBuilder.BindWithSecrets<IdentityClientSettings>(secrets);

        webApplicationBuilder.Services
            .AddSingleton(webApplicationBuilder
            .Bind<FeatureClientConfiguration>());

        webApplicationBuilder.Services
            .AddSingleton(webApplicationBuilder
            .Bind<EventConfiguration>());
    }

    public static T Bind<T>(this WebApplicationBuilder webApplicationBuilder)
    {
        var instance = Activator.CreateInstance(typeof(T));
        webApplicationBuilder.Configuration.GetSection(typeof(T).Name).Bind(instance);
        return (T)instance!;
    }

    private static void BindWithSecrets<T>(this WebApplicationBuilder webApplicationBuilder, IDictionary<string, string> secrets)
        where T : class
    {
        var settings = webApplicationBuilder.Bind<T>();
        settings.InjectSecrets(secrets);
        webApplicationBuilder.Services.AddSingleton(settings);
    }

    private static void ConfigureAuth(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services
            .AddMicrosoftIdentityWebApiAuthentication(webApplicationBuilder.Configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

        webApplicationBuilder.Services.AddAuthorization(config =>
        {
            config.AddPolicy(AuthScheme.Read, policy =>
            {
                policy.RequireRole(AuthRole.Read);
            });

            config.AddPolicy(AuthScheme.Write, policy =>
            {
                policy.RequireRole(AuthRole.Write);
            });

            config.AddPolicy(AuthScheme.Execute, policy =>
            {
                policy.RequireRole(AuthRole.Execute);
            });
        });
    }

    private static CertificateClient GetCertificateClient(this WebApplicationBuilder webApplicationBuilder)
    {
        var keyVaultSettings = webApplicationBuilder.Bind<KeyVaultSettings>();
        return new CertificateClient(new Uri(keyVaultSettings.KeyVaultUrl), new DefaultAzureCredential());
    }
}
