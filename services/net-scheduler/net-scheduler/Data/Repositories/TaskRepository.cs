namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Constants;
using NetScheduler.Data.Models;
using System.Linq.Expressions;

public class TaskRepository : IMongoRepository<ScheduleTask>, ITaskRepository
{
    private readonly ILogger<TaskRepository> _logger;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScheduleTask> _collection;
    private readonly IMongoQueryable<ScheduleTask> _query;

    public TaskRepository(
        IMongoClient mongoClient,
        ILogger<TaskRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoClient, nameof(mongoClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _database = mongoClient.GetDatabase(
            MongoConstants.DatbabaseName);
        _collection = _database.GetCollection<ScheduleTask>(
            MongoConstants.TaskCollectionName);

        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var result = await _collection.DeleteOneAsync(
            x => x.TaskId == id,
            cancellationToken: token);

        return (int)result.DeletedCount;
    }

    public async Task<ScheduleTask> Get(string id, CancellationToken token)
    {
        var task = await _collection.FindAsync(
            x => x.TaskId == id,
            cancellationToken: token);

        return await task.FirstOrDefaultAsync(token);
    }

    public async Task<IEnumerable<ScheduleTask>> GetAll(CancellationToken token)
    {
        var tasks = await _collection.FindAsync(
            _ => true, cancellationToken: token);

        return await tasks.ToListAsync(token);
    }

    public async Task<ScheduleTask> Insert(ScheduleTask entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<ScheduleTask> Update(ScheduleTask entity, CancellationToken token)
    {
        await _collection.ReplaceOneAsync(
            x => x.TaskId == entity.TaskId,
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IEnumerable<ScheduleTask>> Query(
        Expression<Func<ScheduleTask, bool>> query,
        CancellationToken token)
    {
        var result = await _query.Where(query).ToListAsync();

        return result;
    }
}
