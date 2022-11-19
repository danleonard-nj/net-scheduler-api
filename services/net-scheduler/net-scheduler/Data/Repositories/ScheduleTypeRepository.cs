namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Models;

public class ScheduleTypeRepository : IMongoRepository<ScheduleType>
{

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScheduleType> _collection;
    private readonly IMongoQueryable<ScheduleType> _query;
    private readonly ILogger<ScheduleTypeRepository> _logger;


    public ScheduleTypeRepository(
        IMongoClient mongoClient,
        ILogger<ScheduleTypeRepository> logger)
    {
        _database = mongoClient.GetDatabase("Schedule");
        _collection = _database.GetCollection<ScheduleType>("ScheduleType");
        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public Task<int> Delete(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleType> Get(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScheduleType>> GetAll(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleType> Insert(ScheduleType entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleType> Update(ScheduleType entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
