namespace NetScheduler.Tests;

using Microsoft.Extensions.DependencyInjection;

public static class TestExtensions
{
    public static T Resolve<T>(this WebApplicationFixture fixture)
        where T : class
    {
        return fixture
            .Services
            .GetRequiredService<T>();
    }
}