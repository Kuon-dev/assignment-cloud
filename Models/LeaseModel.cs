// LeaseModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

public class LeaseModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [ForeignKey("TenantId")]
    public TenantModel? Tenant { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [ForeignKey("UnitId")]
    public UnitModel? Unit { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RentAmount { get; set; }

      [Column(TypeName = "decimal(18,2)")]
      public decimal SecurityDeposit { get; set; }

      public bool IsActive { get; set; }

      public DateTime CreatedAt { get; set; }

      public DateTime UpdatedAt { get; set; }
  }
}
