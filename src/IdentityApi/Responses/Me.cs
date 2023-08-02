using System;

namespace IdentityApi.Responses;

public class Me
{
	public string Username { get; set; } = default!;
	public string Email { get; set; } = default!;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string Role { get; set; } = default!;
	public string Token { get; set; } = default!;
}