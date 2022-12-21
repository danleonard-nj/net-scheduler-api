using NetScheduler.Data.Entities;
using NetScheduler.Models.Identity;

namespace NetScheduler.Services.Identity.Extensions;

public static class IdentityClientExtensions
{
    public static IdentityClient ToIdentityClient(this IdentityClientModel model)
    {
        return new IdentityClient
        {
            Name = model.Name,
            IdentityClientId = model.IdentityClientId,
            ClientSecret = model.ClientSecret,
            Scopes = model.Scopes,
            GrantType = model.GrantType,
            ClientId = model.ClientId,
        };
    }

    public static IdentityClientModel ToDomain(this IdentityClient identityClient)
    {
        return new IdentityClientModel
        {
            Name = identityClient.Name,
            ClientId = identityClient.ClientId,
            ClientSecret = identityClient.ClientSecret,
            Scopes = identityClient.Scopes,
            GrantType = identityClient.GrantType,
            IdentityClientId = identityClient.IdentityClientId,
        };
    }

    public static IdentityClientModel ToDomain(this CreateIdentityClient model)
    {
        return new IdentityClientModel
        {
            Scopes = model.Scopes,
            ClientSecret = model.ClientSecret,
            ClientId = model.ClientId,
            GrantType = model.GrantType,
            IdentityClientId = Guid.NewGuid().ToString(),
            Name = model.Name
        };
    }
}
