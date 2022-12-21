namespace NetScheduler.Data.Repositories;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NetScheduler.Data.Abstractions;
using NetScheduler.Data.Entities;

public class ScheduleTypeRepository : IMongoRepository<ScheduleTypeItem>
{

    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ScheduleTypeItem> _collection;
    private readonly IMongoQueryable<ScheduleTypeItem> _query;
    private readonly ILogger<ScheduleTypeRepository> _logger;


    public ScheduleTypeRepository(
        IMongoClient mongoClient,
        ILogger<ScheduleTypeRepository> logger)
    {
        _database = mongoClient.GetDatabase("Schedule");
        _collection = _database.GetCollection<ScheduleTypeItem>("ScheduleType");
        _query = _collection.AsQueryable();
        _logger = logger;
    }

    public Task<int> Delete(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleTypeItem> Get(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScheduleTypeItem>> GetAll(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleTypeItem> Insert(ScheduleTypeItem entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ScheduleTypeItem> Replace(ScheduleTypeItem entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
