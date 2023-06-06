using System;

namespace IdentityApi.Models;

public class User
{
	public Guid Id { get; set; }
	public string Username { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string PasswordHash { get; set; } = default!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string Role { get; set; } = default!;
}