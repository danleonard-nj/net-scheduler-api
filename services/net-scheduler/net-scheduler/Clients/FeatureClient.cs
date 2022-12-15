namespace NetScheduler.Clients;

using Flurl.Http;
using Flurl.Http.Configuration;
using NetScheduler.Clients.Abstractions;
using NetScheduler.Clients.Models;
using NetScheduler.Configuration;
using NetScheduler.Configuration.Settings;

public class FeatureClient : IFeatureClient
{
    private readonly IFlurlClient _flurlClient;

    private readonly FeatureClientConfiguration _configuration;
    private readonly ILogger<FeatureClient> _logger;

    public FeatureClient(IFlurlClientFactory flurlClientFactory,
        FeatureClientConfiguration featureClientConfiguration,
        ILogger<FeatureClient> logger)
    {
        ArgumentNullException.ThrowIfNull(flurlClientFactory, nameof(flurlClientFactory));
        ArgumentNullException.ThrowIfNull(featureClientConfiguration, nameof(featureClientConfiguration));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _configuration = featureClientConfiguration;
        _flurlClient = flurlClientFactory.Get(new Flurl.Url(featureClientConfiguration.BaseUrl));
    }

    public async Task<bool> EvaluateFeature(string featureKey)
    {
        _logger.LogInformation(
            "{@Method}: {@FeatureKey}: Evaluating feature",
            Caller.GetName(),
            featureKey);

        var feature = await _flurlClient
            .Request("api/feature/evaluate")
            .AppendPathSegment(featureKey)
            .WithHeader("X-Api-Key", _configuration.ApiKey)
            .GetJsonAsync<EvaluateFeatureResponseModel>();

        _logger.LogInformation(
           "{@Method}: {@FeatureKey}: {@Value}: Feature value",
           Caller.GetName(),
           featureKey,
           feature);

        return feature.Value;
    }
}
