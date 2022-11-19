namespace NetScheduler.Tests;

using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc.Testing;

public class WebApplicationFixture : WebApplicationFactory<Program>
{
    private readonly Fixture autoFixture = (Fixture)new Fixture()
        .Customize(new AutoMoqCustomization());

    public Fixture AutoFixture { get => autoFixture; }

    public WebApplicationFixture()
    {
    }
}
