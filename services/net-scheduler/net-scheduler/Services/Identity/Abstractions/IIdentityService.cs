namespace NetScheduler.Services.Identity.Abstractions;
using NetScheduler.Models.Identity;

public interface IIdentityService
{
    Task<TokenResponseModel> GetClientToken(CancellationToken cancellationToken);

    Task<TokenResponseModel> GetUserToken(string username, string password, CancellationToken cancellationToken);
}
