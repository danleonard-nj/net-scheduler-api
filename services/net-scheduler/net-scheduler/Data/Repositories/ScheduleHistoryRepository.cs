namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Constants;
using NetScheduler.Data.Entities;
using System.Linq.Expressions;


public class ScheduleHistoryRepository : IScheduleHistoryRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScheduleHistoryItem> _collection;
    private readonly IMongoQueryable<ScheduleHistoryItem> _query;

    private readonly ILogger<ScheduleHistoryRepository> _logger;

    public ScheduleHistoryRepository(
        IMongoClient mongoClient,
        ILogger<ScheduleHistoryRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(mongoClient, nameof(mongoClient));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _database = mongoClient.GetDatabase(
            MongoConstants.DatbabaseName);
        _collection = _database.GetCollection<ScheduleHistoryItem>(
            MongoConstants.TaskCollectionName);

        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public async Task<IEnumerable<ScheduleHistoryItem>> GetScheduleHistoryByCreatedDateRangeAsync(
        int startCreatedDate,
        int endCreatedDate,
        CancellationToken token)
    {
        var builder = Builders<ScheduleHistoryItem>.Filter;

        var queryFilter = builder.And(
            builder.Gte(x => x.CreatedDate, startCreatedDate),
            builder.Lte(x => x.CreatedDate, endCreatedDate));

        return await _collection
            .Find(queryFilter)
            .SortByDescending(x => x.CreatedDate)
            .ToListAsync(token);
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var result = await _collection.DeleteOneAsync(
            x => x.ScheduleHistoryId == id,
            cancellationToken: token);

        return (int)result.DeletedCount;
    }

    public async Task<ScheduleHistoryItem> Get(string id, CancellationToken token)
    {
        var task = await _collection.FindAsync(
            x => x.ScheduleHistoryId == id,
            cancellationToken: token);

        return await task.FirstOrDefaultAsync(token);
    }

    public async Task<IEnumerable<ScheduleHistoryItem>> GetAll(CancellationToken token)
    {
        var tasks = await _collection.FindAsync(
            _ => true, cancellationToken: token);

        return await tasks.ToListAsync(token);
    }

    public async Task<ScheduleHistoryItem> Insert(ScheduleHistoryItem entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<ScheduleHistoryItem> Replace(ScheduleHistoryItem entity, CancellationToken token)
    {
        await _collection.ReplaceOneAsync(
            x => x.ScheduleHistoryId == entity.ScheduleHistoryId,
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IEnumerable<ScheduleHistoryItem>> Query(
        Expression<Func<ScheduleHistoryItem, bool>> query,
        CancellationToken token)
    {
        var result = await _query.Where(query).ToListAsync();

        return result;
    }
}