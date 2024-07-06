using Microsoft.EntityFrameworkCore;
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