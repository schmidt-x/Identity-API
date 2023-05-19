using System;

namespace IdentityApi.Models;

public class User
{
	public Guid Id { get; set; }
	public string Username { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string Role { get; set; }
}