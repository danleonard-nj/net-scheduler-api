namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Models;
using System.Linq.Expressions;

public class ScheduleRepository : IMongoRepository<Schedule>, IScheduleRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Schedule> _collection;
    private readonly IMongoQueryable<Schedule> _query;
    private readonly ILogger<ScheduleRepository> _logger;


    public ScheduleRepository(
        IMongoClient mongoClient,
        ILogger<ScheduleRepository> logger)
    {
        _database = mongoClient.GetDatabase("Schedule");
        _collection = _database.GetCollection<Schedule>("Schedule");
        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public async Task<Schedule> Get(string id, CancellationToken token)
    {
        var schedule = await _collection.FindAsync(
            x => x.ScheduleId == id,
            cancellationToken: token);

        return await schedule.FirstOrDefaultAsync(token);
    }

    public async Task<Schedule> Update(Schedule entity, CancellationToken token)
    {
        var result = await _collection.ReplaceOneAsync(
            x => x.ScheduleId == entity.ScheduleId,
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<int> Delete(string id, CancellationToken token)
    {
        var result = await _collection.DeleteOneAsync(
            x => x.ScheduleId == id,
            cancellationToken: token);

        return (int)result.DeletedCount;
    }

    public async Task<Schedule> Insert(Schedule entity, CancellationToken token)
    {
        await _collection.InsertOneAsync(
            entity,
            cancellationToken: token);

        return entity;
    }

    public async Task<IEnumerable<Schedule>> GetAll(CancellationToken token)
    {
        var all = await _collection.FindAsync(
            _ => true,
            cancellationToken: token);

        return await all.ToListAsync(token);
    }

    public async Task<IEnumerable<Schedule>> Query(Expression<Func<Schedule, bool>> query, CancellationToken token)
    {
        var result = await _query.Where(query).ToListAsync();

        return result;
    }
}
