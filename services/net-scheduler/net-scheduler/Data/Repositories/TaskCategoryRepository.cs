namespace NetScheduler.Data.Repositories;

using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Constants;
using NetScheduler.Data.Entities;
using System.Diagnostics.CodeAnalysis;

public class TaskCategoryRepository : ITaskCategoryRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<TaskCategoryItem> _collection;
    private readonly IMongoQueryable<TaskCategoryItem> _query;
    private readonly ILogger<TaskCategoryRepository> _logger;

    public TaskCategoryRepository(
        IMongoClient mongoClient,
        ILogger<TaskCategoryRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoClient, nameof(mongoClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _database = mongoClient.GetDatabase(
            MongoConstants.DatbabaseName);
        _collection = _database.GetCollection<TaskCategoryItem>(
            MongoConstants.TaskCategoryCollectionName);

        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public async Task<TaskCategoryItem> Get(string id, CancellationToken token)
    {
        return await _query
            .Where(x => x.CategoryId == id)
            .FirstOrDefaultAsync(token);
    }

    public async Task<TaskCategoryItem> Replace(TaskCategoryItem entity, CancellationToken token)
    {
        var filter = Builders<TaskCategoryItem>.Filter;

        var queryFilter = filter.Eq(x => x.CategoryId, entity.CategoryId); 
        
        await _collection.ReplaceOneAsync(
            queryFilter,
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var filter = Builders<TaskCategoryItem>.Filter;

        var queryFilter = filter.Eq(x => x.CategoryId, id);

        var result = await _collection.DeleteOneAsync(
            queryFilter,
            cancellationToken: token);

        return (int)result.DeletedCount;
    }

    public async Task<TaskCategoryItem> Insert(TaskCategoryItem entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IEnumerable<TaskCategoryItem>> GetAll(CancellationToken token)
    {
        return await _query
            .ToListAsync(token);
    }
}
