namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Constants;
using NetScheduler.Data.Entities;
using System.Linq.Expressions;

public class TaskRepository : ITaskRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<TaskItem> _collection;
    private readonly IMongoQueryable<TaskItem> _query;

    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(
        IMongoClient mongoClient,
        ILogger<TaskRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoClient, nameof(mongoClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _database = mongoClient.GetDatabase(
            MongoConstants.DatbabaseName);
        _collection = _database.GetCollection<TaskItem>(
            MongoConstants.TaskCollectionName);

        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetTasksAsync(
        IEnumerable<string> taskIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(taskIds, nameof(taskIds));

        var queryFilter = Builders<TaskItem>
            .Filter
            .In(x => x.TaskId, taskIds);

        return await _collection
            .Find(queryFilter)
            .ToListAsync();
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var result = await _collection.DeleteOneAsync(
            x => x.TaskId == id,
            cancellationToken: token);

        return (int)result.DeletedCount;
    }

    public async Task<TaskItem> Get(string id, CancellationToken token)
    {
        var task = await _collection.FindAsync(
            x => x.TaskId == id,
            cancellationToken: token);

        return await task.FirstOrDefaultAsync(token);
    }

    public async Task<IEnumerable<TaskItem>> GetAll(CancellationToken cancellationToken)
    {
        return await _collection
            .Find(_ => true)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskItem> Insert(TaskItem entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<TaskItem> Replace(TaskItem entity, CancellationToken token)
    {
        await _collection.ReplaceOneAsync(
            x => x.TaskId == entity.TaskId,
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IEnumerable<TaskItem>> Query(
        Expression<Func<TaskItem, bool>> query,
        CancellationToken token)
    {
        var result = await _query.Where(query).ToListAsync();

        return result;
    }

    public async Task<TaskItem?> GetTaskByNameAsync(
        string taskName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(taskName))
        {
            throw new ArgumentNullException(nameof(taskName));
        }

        var queryFilter = Builders<TaskItem>
            .Filter
            .Eq(x => x.TaskName, taskName);

        return await _collection
            .Find(queryFilter)
            .FirstOrDefaultAsync(cancellationToken);
    }
}