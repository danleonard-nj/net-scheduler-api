namespace NetScheduler.Controllers;

using NetScheduler.Data.Entities;
using NetScheduler.Models.Tasks;

public static class TaskCategoyExtensions
{
    public static TaskCategoryItem ToEntity(this TaskCategoryModel model)
               => new()
               {
                   CategoryId = model.CategoryId,
                   CategoryName = model.CategoryName,
                   CreatedDate = model.CreatedDate,
                   ModifiedDate = model.ModifiedDate
               };

    public static TaskCategoryModel ToDomain(this TaskCategoryItem entity)
               => new()
               {
                   CategoryId = entity.CategoryId,
                   CategoryName = entity.CategoryName,
                   CreatedDate = entity.CreatedDate,
                   ModifiedDate = entity.ModifiedDate
               };
}