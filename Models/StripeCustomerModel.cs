
// StripeCustomerModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

public class StripeCustomerModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

      [ForeignKey("UserId")]
      public UserModel? User { get; set; }

      [Required]
      public string StripeCustomerId { get; set; } = string.Empty;

      public DateTime CreatedAt { get; set; }

      public DateTime UpdatedAt { get; set; }
  }
}
