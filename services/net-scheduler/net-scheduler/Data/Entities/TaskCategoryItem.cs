namespace NetScheduler.Data.Entities;

public class TaskCategoryItem
{
    public string CategoryId { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public DateTime? ModifiedDate { get; set; }

    public DateTime? CreatedDate { get; set; } = null!;
}
