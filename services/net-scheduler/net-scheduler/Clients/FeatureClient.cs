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
        if (string.IsNullOrWhiteSpace(featureKey))
        {
            throw new ArgumentNullException(nameof(featureKey));
        }

        _logger.LogInformation(
            "{@Method}: {@FeatureHeaderKey}: {@FeatureApiKey}: {@FeatureKey}: Evaluating feature",
            Caller.GetName(),
            _configuration.ApiKeyHeader,
            _configuration.ApiKey,
            featureKey);

        var feature = await _flurlClient
            .Request("api/feature/evaluate")
            .AppendPathSegment(featureKey)
            .WithHeader(_configuration.ApiKeyHeader, _configuration.ApiKey)
            .GetJsonAsync<EvaluateFeatureResponseModel>();

        _logger.LogInformation(
           "{@Method}: {@FeatureKey}: {@Value}: Feature value",
           Caller.GetName(),
           featureKey,
           feature);

        return feature.Value;
    }
}
