namespace NetScheduler.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetScheduler.Data.Entities;

public interface ITaskCategoryService
{
    Task<int> DeleteTaskCategoryAsync(string id, CancellationToken token);
    Task<TaskCategoryItem> GetTaskCategoryAsync(string id, CancellationToken token);
    Task<IEnumerable<TaskCategoryItem>> GetTaskCategoriesAsync(CancellationToken token);
    Task<TaskCategoryItem> InsertTaskAsync(TaskCategoryItem entity, CancellationToken token);
    Task<TaskCategoryItem> ReplaceTaskAsync(TaskCategoryItem entity, CancellationToken token);
}