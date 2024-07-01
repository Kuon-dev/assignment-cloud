// MaintenanceRequestModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

public class MaintenanceRequestModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public TenantModel? Tenant { get; set; }

    [Required]
    public Guid PropertyId { get; set; }

    [ForeignKey("PropertyId")]
    public PropertyModel? Property { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public MaintenanceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
      public ICollection<MaintenanceTaskModel>? MaintenanceTasks { get; set; }
  }

  public enum MaintenanceStatus
  {
      Pending,
      InProgress,
      Completed,
      Cancelled
  }
}
