// MaintenanceTaskModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Cloud.Models
{

    public class MaintenanceTaskModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RequestId { get; set; }

        [ForeignKey("RequestId")]
        public MaintenanceRequestModel? Request { get; set; }

        [Required]
        public Guid StaffId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCost { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public TaskStatus Status { get; set; }
    }


    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
