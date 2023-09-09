using NetScheduler.Data.Entities;
using NetScheduler.Services;

namespace NetScheduler.Data.Repositories;

public class TaskCategoryService : ITaskCategoryService
{
    public Task<int> DeleteTaskCategoryAsync(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TaskCategoryItem> GetTaskCategoryAsync(string id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TaskCategoryItem>> GetTaskCategoriesAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TaskCategoryItem> InsertTaskAsync(TaskCategoryItem entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TaskCategoryItem> ReplaceTaskAsync(TaskCategoryItem entity, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
