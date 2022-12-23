namespace NetScheduler.Services.Identity.Abstractions;

using System.Threading;
using System.Threading.Tasks;

public interface IIdentityService
{
    Task<string> GetAccessTokenAsync(string clientId, CancellationToken token = default);
    Task<object> GetAuthorizationHeadersAsync(string clientId, CancellationToken cancellationToken = default);
}