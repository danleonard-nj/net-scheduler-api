using NetScheduler.Data.Entities;

namespace NetScheduler.Data.Abstractions;
public interface ITaskCategoryRepository : IMongoRepository<TaskCategoryItem>
{
}
