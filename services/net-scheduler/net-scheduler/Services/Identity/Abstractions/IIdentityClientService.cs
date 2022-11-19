namespace NetScheduler.Services.Identity.Abstractions;

using NetScheduler.Models.Identity;

public interface IIdentityClientService
{
    Task DeleteIdentityClient(string id, CancellationToken token);

    Task<IdentityClientModel> Get(string id, CancellationToken token);

    Task<IEnumerable<IdentityClientModel>> GetAll(CancellationToken token);

    Task<IdentityClientModel> Insert(IdentityClientModel entity, CancellationToken token);

    Task<IdentityClientModel> Update(IdentityClientModel entity, CancellationToken token);
}
