namespace NetScheduler.Data.Abstractions;

using NetScheduler.Data.Entities;

public interface IIdentityClientRepository
{
    Task<int> Delete(string id, CancellationToken token);
    Task<IdentityClient> Get(string id, CancellationToken token);
    Task<IEnumerable<IdentityClient>> GetAll(CancellationToken token);
    Task<IdentityClient> Insert(IdentityClient entity, CancellationToken token);
    Task<IdentityClient> Replace(IdentityClient entity, CancellationToken token);
}
