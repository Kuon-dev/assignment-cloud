// OwnerModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud.Models {
  public class OwnerModel {
	[Key]
	public Guid Id { get; set; }

	[Required]
	public String UserId { get; set; }

	[ForeignKey("UserId")]
	public UserModel? User { get; set; }

	// Navigation properties
	public ICollection<PropertyModel>? Properties { get; set; }
  }
}