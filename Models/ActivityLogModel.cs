// ActivityLogModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Models {
  public class ActivityLogModel {
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid UserId { get; set; }

	[Required]
	public string Action { get; set; }

	[Required]
	public DateTime Timestamp { get; set; }

	public string Details { get; set; }
  }
}