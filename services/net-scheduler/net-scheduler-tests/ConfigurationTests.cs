namespace NetScheduler.Tests;

using Microsoft.Extensions.Configuration;
using NetScheduler.Configuration.Settings;
using System;
using Xunit;

public class ConfigurationTests : IClassFixture<WebApplicationFixture>
{
    private readonly WebApplicationFixture _fixture;

    public ConfigurationTests(WebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetRedisSettings_ReturnsValidConfiguration()
    {
        // Arrange
        var configuration = _fixture
            .Resolve<IConfiguration>();

        var redisConnectionString = configuration
            .GetConnectionString("Redis");

        Assert.NotNull(redisConnectionString);
    }

    [Fact]
    public void GetIdentityClientSettings_ReturnsValidConfiguration()
    {
        // Arrange
        var settings = _fixture
            .Resolve<IdentityClientSettings>();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.BaseUrl);
        Assert.NotNull(settings.ClientId);
        Assert.NotNull(settings.ClientSecret);
    }

    [Fact]
    public void GetFeatureClientSettings_ReturnsValidConfiguration()
    {
        // Arrange
        var settings = _fixture
            .Resolve<FeatureClientConfiguration>();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.BaseUrl);
        Assert.NotNull(settings.ApiKey);
    }

    [Fact]
    public void GetMongoSettings_ReturnsValidConfiguration()
    {
        // Arrange
        var configuration = _fixture
            .Resolve<IConfiguration>();

        var settings = Activator
            .CreateInstance<MongoConfiguration>();

        configuration
            .GetSection(typeof(MongoConfiguration).Name)
            .Bind(settings);

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.ConnectionString);
    }

    [Fact]
    public void GetKeyVault_ReturnsValidConfiguration()
    {
        // Arrange
        var configuration = _fixture
            .Resolve<IConfiguration>();

        var settings = Activator
            .CreateInstance<KeyVaultSettings>();

        configuration
            .GetSection(typeof(KeyVaultSettings).Name)
            .Bind(settings);

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.KeyVaultUrl);
    }
}
