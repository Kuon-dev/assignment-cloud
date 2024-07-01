// AdminModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {

  public class AdminModel
  {
      [Key]
      public Guid Id { get; set; }

      [Required]
      public Guid UserId { get; set; }

      [ForeignKey("UserId")]
      public UserModel? User { get; set; }
  }
}
