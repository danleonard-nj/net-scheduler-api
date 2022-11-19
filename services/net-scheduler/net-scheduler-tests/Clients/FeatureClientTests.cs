using Xunit;
using NetScheduler.Tests;
using Microsoft.Extensions.DependencyInjection;
using NetScheduler.Configuration.Settings;
using Flurl.Http;
using System.Threading.Tasks;

namespace NetScheduler.Tests.Clients
{
    public class FeatureClientTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _fixture;

        public FeatureClientTests(WebApplicationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void FeatureClient_ClientConfiguration_ReturnSuccess()
        {
            // Arrange
            var configuration = _fixture
                .Services
                .GetRequiredService<FeatureClientConfiguration>();

            // Assert
            Assert.NotNull(configuration.ApiKey);
            Assert.NotNull(configuration.BaseUrl);
        }

        [Fact]
        public async Task FeatureClient_HealthAlive_ReturnsSuccess()
        {
            // Arrange
            var configuration = _fixture
                   .Services
                   .GetRequiredService<FeatureClientConfiguration>();

            var client = new FlurlClient(configuration.BaseUrl);

            // Act
            var result = await client
                .Request("api/health/alive")
                .GetAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task FeatureClient_HealthReady_ReturnsSuccess()
        {
            // Arrange
            var configuration = _fixture
                   .Services
                   .GetRequiredService<FeatureClientConfiguration>();

            var client = new FlurlClient(configuration.BaseUrl);

            // Act
            var result = await client
                .Request("api/health/ready")
                .GetAsync();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }
    }
}