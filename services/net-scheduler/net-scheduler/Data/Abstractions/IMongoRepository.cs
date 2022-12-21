namespace NetScheduler.Data.Abstractions;

public interface IMongoRepository<T>
{
    Task<T> Get(string id, CancellationToken token);

    Task<T> Replace(T entity, CancellationToken token);

    Task<int> Delete(string id, CancellationToken token);

    Task<T> Insert(T entity, CancellationToken token);

    Task<IEnumerable<T>> GetAll(CancellationToken token);
}
