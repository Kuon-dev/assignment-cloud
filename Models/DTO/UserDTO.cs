/*using Microsoft.EntityFrameworkCore;*/
using System.ComponentModel.DataAnnotations;
using Cloud.Models;

// DTO for user information
public class UserInfoDto
{
	public Guid Id { get; set; }
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public UserRole Role { get; set; }
	public bool IsVerified { get; set; }
	public string? ProfilePictureUrl { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public OwnerInfoDto? Owner { get; set; }
	public TenantInfoDto? Tenant { get; set; }
	public AdminInfoDto? Admin { get; set; }
}

public class OwnerInfoDto
{
	public Guid Id { get; set; }
}

public class TenantInfoDto
{
	public Guid Id { get; set; }
}

public class AdminInfoDto
{
	public Guid Id { get; set; }
}

public class UpdateUserDto
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? CurrentPassword { get; set; }
	public string? NewPassword { get; set; }
}

public class CreateUserDto
{
	[Required]
	public string FirstName { get; set; } = string.Empty;

	[Required]
	public string LastName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	public int Role { get; set; }

	public string? ProfilePictureUrl { get; set; }

	[Required]
	public string Password { get; set; } = string.Empty;
}