namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Constants;
using NetScheduler.Data.Entities;

public class IdentityClientRepository : IMongoRepository<IdentityClient>, IIdentityClientRepository
{
    private readonly IMongoCollection<IdentityClient> _collection;
    private readonly ILogger _logger;

    public IdentityClientRepository(
        IMongoClient mongoClient,
        ILogger<IdentityClientRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoClient, nameof(mongoClient));

        var database = mongoClient.GetDatabase(
            MongoConstants.DatbabaseName);

        _collection = database.GetCollection<IdentityClient>(
            MongoConstants.IdentityClientCollectionName);

        _logger = logger;
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var result = await _collection.DeleteOneAsync(
            x => x.IdentityClientId == id,
            token);

        return (int)result.DeletedCount;
    }

    public async Task<IdentityClient> Get(string id, CancellationToken token)
    {
        var client = await _collection.FindAsync(
            x => x.IdentityClientId == id,
            cancellationToken: token);

        return await client.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<IdentityClient>> GetAll(CancellationToken token)
    {
        var clients = await _collection.FindAsync(
            _ => true,
            cancellationToken: token);

        return await clients.ToListAsync();
    }

    public async Task<IdentityClient> Insert(IdentityClient entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IdentityClient> Replace(IdentityClient entity, CancellationToken token)
    {
        await _collection.ReplaceOneAsync(
            x => x.IdentityClientId == entity.IdentityClientId,
            entity,
            cancellationToken: token);

        return entity;
    }
}
