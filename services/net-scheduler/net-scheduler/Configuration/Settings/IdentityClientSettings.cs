namespace NetScheduler.Configuration.Settings;

using NetScheduler.Configuration.Attributes;

public class IdentityClientSettings
{
    public string? BaseUrl { get; set; } = null!;

    public string? ClientId { get; set; } = null!;

    [KeyVaultSecret("KubeToolsClientSecret")]
    public string? ClientSecret { get; set; } = null!;
}
