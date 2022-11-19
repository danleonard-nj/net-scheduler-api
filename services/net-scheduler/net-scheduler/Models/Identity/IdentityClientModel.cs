namespace NetScheduler.Models.Identity;

using System.Text.Json.Serialization;

public class IdentityClientModel
{
    public string Name { get; set; } = null!;

    public string IdentityClientId { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string Scopes { get; set; } = null!;

    public string GrantType { get; set; } = null!;
}
