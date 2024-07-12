namespace Cloud.Models.DTO
{
	public class MaintenanceTaskDto
	{
		public Guid Id { get; set; }
		public Guid RequestId { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal? EstimatedCost { get; set; }
		public decimal? ActualCost { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? CompletionDate { get; set; }
		public TaskStatus Status { get; set; }
	}

	public class CreateMaintenanceTaskDto
	{
		public Guid RequestId { get; set; }
		public string Description { get; set; } = string.Empty;
		public decimal EstimatedCost { get; set; }
	}

	public class UpdateMaintenanceTaskDto
	{
		public string? Description { get; set; }
		public decimal? EstimatedCost { get; set; }
		public decimal? ActualCost { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? CompletionDate { get; set; }
		public Cloud.Models.TaskStatus Status { get; set; }
	}
}