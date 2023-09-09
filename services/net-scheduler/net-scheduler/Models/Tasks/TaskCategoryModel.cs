namespace NetScheduler.Models.Tasks;

public class TaskCategoryModel
{
    public string CategoryId { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public DateTime? ModifiedDate { get; set; } 

    public DateTime? CreatedDate { get; set; } = null!;

}